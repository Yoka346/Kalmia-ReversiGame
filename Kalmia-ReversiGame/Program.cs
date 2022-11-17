using System.Text;

using Kalmia_Game.SDLWrapper;
using Kalmia_Game.View;
using Kalmia_Game.View.Scenes;

namespace Kalmia_Game
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (GlobalConfig.Instance.WorkDirPath is not null && GlobalConfig.Instance.WorkDirPath != string.Empty)
                Environment.CurrentDirectory = GlobalConfig.Instance.WorkDirPath;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            AudioMixer.Init();
            Application.Run(new MainForm(() => new TitleScene()));
            AudioMixer.Quit();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
            => HandleException(e.Exception);

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                HandleException(ex);
        }

        static void HandleException(Exception ex)
        {
            using var fs = CreateErrorLogFile(out string path);
            using var sw = new StreamWriter(fs);
            sw.WriteLine(ex.ToString());
            ShowErrorMsgBox(path);
        }

        static void ShowErrorMsgBox(string logPath)
        {
            var msg = new StringBuilder("�\���󂲂����܂���B�Q�[�����\�������I�����܂����B\n");
            msg.Append("�G���[�̏ڍׂ�\"").Append(logPath).Append("\"�ɋL�q����Ă��܂��B\n");
            msg.Append("���萔�ł����A���̃t�@�C�����J���҂ɑ����Ă���������ƍK���ł��B");
            MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static FileStream CreateErrorLogFile(out string path)
        {
            var i = 0;
            do
                path = string.Format(FilePath.ErrorLogPath, i++);
            while (File.Exists(path));
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }
    }
}