using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Kalmia_Game.Model.Reversi;
using Kalmia_Game.Utils;

namespace Kalmia_Game.Model.Engine
{
    internal enum EngineState
    {
        StartUp,
        USIOK,
        Ready,
        Playing,
        GameOver,
        WaitForQuit,
        Quit
    }

    internal enum EvalScoreType
    {
        WinRate,
        DiscDiff,
        Other
    }

    class KalmiaInfo
    {
        public int? MultiPV { get; set; }
        public int? Nps { get; set; }
        public long? NodeCount { get; set; }
        public int? Depth { get; set; }
        public double? EvalScore { get; set; }
        public List<BoardCoordinate> PV { get; } = new();
    }

    delegate void USIInfoEventHandler(KalmiaEngine engine, KalmiaInfo info);
    delegate void USIBestMoveEventHandler(KalmiaEngine sender, BoardCoordinate move);

    /// <summary>
    /// KalmiaとUSIプロトコルでやり取りをするクラス.
    /// </summary>
    internal class KalmiaEngine
    {
        public string? Name { get; private set; }
        public string? Author { get; private set; }
        public EngineState? State { get; private set; }
        public EvalScoreType ScoreType { get; private set; }
        public double MinScore { get; private set; }
        public double MaxScore { get; private set; }

        /// <summary>
        /// エンジンが正常終了したかどうか.
        /// </summary>
        public bool HasQuitSuccessfully { get; private set; }

        /// <summary>
        /// エンジンが予期せず終了したかどうか. 予期しない終了とは, QuitメソッドまたはKillメソッド以外での終了のこと.
        /// </summary>
        public bool HasQuitUnexpectedly { get; private set; }

        /// <summary>
        /// エンジンが強制終了されたかどうか.
        /// </summary>
        public bool WasKilled { get; private set; }

        public bool WaitForBeingKilled { get; private set; }

        public bool HasExited => this.process is not null && this.process.HasExited;

        public event EventHandler OnTerminated = delegate { };
        public event USIInfoEventHandler OnInformationRecieved = delegate { };
        public event USIBestMoveEventHandler OnBestMoveRecieved = delegate { };

        readonly string ENGINE_PATH;
        readonly string ENGINE_ARGS;
        readonly string ENGINE_WORK_DIR;
        readonly Dictionary<string, Action<IgnoreSpaceStringReader>> COMMANDS = new();

        Process? process;

        public KalmiaEngine(string enginePath, string args = "", string workDir = "")
        {
            this.ENGINE_PATH = enginePath;
            this.ENGINE_ARGS = args;
            this.ENGINE_WORK_DIR = workDir;
            InitCommands();
        }

        void InitCommands()
        {
            this.COMMANDS["id"] = ExecuteIDCommand;
            this.COMMANDS["usiok"] = ExecuteUSIOKCommand;
            this.COMMANDS["readyok"] = ExecuteReadyOKCommand;
            this.COMMANDS["bestmove"] = ExecuteBestMoveCommand;
            this.COMMANDS["info"] = ExecuteInfoCommand;
            this.COMMANDS["option"] = ExecuteOptionCommand;
            this.COMMANDS["scoretype"] = ExecuteScoreTypeCommand;
        }

        public void Run()
        {
            StartProcess();

            if (this.process is null)
                throw new NullReferenceException("Failed to execute engine process.");

            SendCommand("usi");
            while (!this.process.HasExited && this.State != EngineState.USIOK) ;

            SendCommand("isready");
            while (!this.process.HasExited && this.State != EngineState.Ready) ;
        }

        public void Quit()
        {
            const int TIMEOUT_MS = 10000;

            if (this.process is null || this.process.HasExited)
                return;

            SendCommand("quit");
            this.State = EngineState.WaitForQuit;
            if (this.process.WaitForExit(TIMEOUT_MS))
            {
                this.HasQuitSuccessfully = true;
                this.State = EngineState.Quit;
                this.OnTerminated.Invoke(this, EventArgs.Empty);
            }
        }

        public void Kill()
        {
            if (this.process is null || this.process.HasExited)
                return;

            this.WaitForBeingKilled = true;
            this.process.Kill();
            this.WasKilled = true;
            this.WaitForBeingKilled = false;
            this.OnTerminated.Invoke(this, EventArgs.Empty);
        }

        public void SetOption<T>(string name, T value) => this.SendCommand($"setoption name {name} value {value?.ToString()?.ToLower()}");

        public void SetPosition(Position pos) => SendCommand($"position sfen {pos.ToSfenString()}");

        public void Go() { SendCommand("score_scale_and_type"); SendCommand("go"); }
        public void GoPonder() { SendCommand("score_scale_and_type"); SendCommand("go ponder"); }
        public void StopGo() => SendCommand("stop");

        bool StartProcess()
        {
            var psi = new ProcessStartInfo();
            psi.FileName = this.ENGINE_PATH;
            psi.Arguments = this.ENGINE_ARGS;
            if (this.ENGINE_WORK_DIR != string.Empty)
                psi.WorkingDirectory = this.ENGINE_WORK_DIR;
            else
                psi.WorkingDirectory = Directory.GetParent(this.ENGINE_PATH)?.ToString();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            this.process = Process.Start(psi);
            if (this.process is null)
                return false;

            this.process.EnableRaisingEvents = true;
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.Exited += Process_Exited;
            this.process.BeginOutputReadLine();
            return true;
        }

        void SendCommand(string cmd)
        {
            this.process?.StandardInput.WriteLine(cmd);
            Debug.WriteLine($"GUI2Engine: {cmd}");
        }

        void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            Debug.WriteLine($"Engine2GUI: {e.Data}");
            var strReader = new IgnoreSpaceStringReader(e.Data);
            this.COMMANDS.TryGetValue(strReader.Read(), out Action<IgnoreSpaceStringReader>? handler);
            if (handler is not null)
                handler(strReader);
        }

        void Process_Exited(object? sender, EventArgs e)
        {
            if (this.State != EngineState.WaitForQuit && this.State != EngineState.Quit && !this.WaitForBeingKilled && !this.WasKilled)
                this.HasQuitUnexpectedly = true;
            OnTerminated.Invoke(this, EventArgs.Empty);
        }

        void ExecuteIDCommand(IgnoreSpaceStringReader strReader)
        {
            var token = strReader.Read();
            if (strReader.Peek() == -1)
                return;

            if (token == "name")
                this.Name = strReader.Read();
            else if (token == "author")
                this.Author = strReader.Read();
        }

        void ExecuteUSIOKCommand(IgnoreSpaceStringReader strReader) => this.State = EngineState.USIOK;

        void ExecuteReadyOKCommand(IgnoreSpaceStringReader strReader) => this.State = EngineState.Ready;

        void ExecuteBestMoveCommand(IgnoreSpaceStringReader strReader)
            => this.OnBestMoveRecieved.Invoke(this, Position.StringToBoardCoordinate(strReader.Read()));

        void ExecuteInfoCommand(IgnoreSpaceStringReader strReader)
        {
            var info = new KalmiaInfo();
            var readPV = false;
            while (strReader.Peek() != -1)
            {
                var token = strReader.Read();

                if (readPV)
                {
                    var coord = Position.StringToBoardCoordinate(token);
                    if (coord == BoardCoordinate.Null)
                        break;
                    info.PV.Add(coord);
                    continue;
                }

                switch (token)
                {
                    case "score":
                        double score;
                        if (double.TryParse(strReader.Read(), out score))
                            info.EvalScore = score;
                        break;

                    case "depth":
                        int depth;
                        if (int.TryParse(strReader.Read(), out depth))
                            info.Depth = depth;
                        break;

                    case "nps":
                        int nps;
                        if (int.TryParse(strReader.Read(), out nps))
                            info.Nps = nps;
                        break;

                    case "nodes":
                        long nodes;
                        if (long.TryParse(strReader.Read(), out nodes))
                            info.NodeCount = nodes;
                        break;

                    case "multipv":
                        int multiPV;
                        if (int.TryParse(strReader.Read(), out multiPV))
                            info.MultiPV = multiPV;
                        break;

                    case "pv":
                        readPV = true;
                        break;
                }
            }
            this.OnInformationRecieved.Invoke(this, info);
        }

        void ExecuteOptionCommand(IgnoreSpaceStringReader strReader)
        {
            // 今のところ特にやることなし.
        }

        void ExecuteScoreTypeCommand(IgnoreSpaceStringReader strReader)
        {
            this.ScoreType = strReader.Read() switch
            {
                "WP" => EvalScoreType.WinRate,
                "stone" => EvalScoreType.DiscDiff,
                _ => EvalScoreType.Other
            };

            string token;
            while ((token = strReader.Read()) != "\0")
                if (token == "min" && double.TryParse(strReader.Read(), out double min))
                    this.MinScore = min;
                else if (token == "max" && double.TryParse(strReader.Read(), out double max))
                    this.MaxScore = max;
        }
    }
}
