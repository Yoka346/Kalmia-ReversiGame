using System.Diagnostics;
using System.IO;

using Kalmia_Game.SDLWrapper;

namespace Kalmia_Game
{
    internal static class GlobalSE
    {
        const string CUSOR_SE_FILE_NAME = "cursor.ogg";
        const string BUTTON_PRESS_SE_FILE_NAME = "button_press.ogg";
        const string POP_UP_SE_FILE_NAME = "pop_up.ogg";

        public static MixerChunk CursorSE { get; }
        public static MixerChunk ButtonPressSE { get; }

        static GlobalSE()
        {
            if (!File.Exists($"{FilePath.SEDirPath}{CUSOR_SE_FILE_NAME}"))
                Debug.WriteLine("fairuganai ");
            CursorSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}{CUSOR_SE_FILE_NAME}");
            ButtonPressSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}{BUTTON_PRESS_SE_FILE_NAME}");
        }

        public static void DisposeAll()
        {
            CursorSE.Dispose();
            ButtonPressSE.Dispose();
        }
    }
}
