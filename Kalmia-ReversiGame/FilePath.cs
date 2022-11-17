namespace Kalmia_Game
{
    internal static class FilePath
    {
        public static string GlobalConfigFilePath { get; } = "config.json";

        // SDL
        public const string SDL_DLL_PATH = $"SDL2.dll";
        public const string SDL_MIXER_DLL_PATH = $"SDL2_mixer.dll";

        // Game
        public static string GameDirPath { get; } = "Game/";
        public static string DifficultyDirPath => $"{GameDirPath}Difficulty";
        public static string GameConfigFilePath => $"{GameDirPath}game_config.json";
        public static string GameStatisticFilePath => $"{GameDirPath}game_statistic.json";
        public static string GameRecordFilePath => $"{GameDirPath}game_record.json";

        // Engine
        public static string EngineDirPath { get; } = "Engine/";

        // Graph
        public static string GraphDirPath { get; } = "Graph/";

        // SE
        public static string SEDirPath { get; } = "SE/";

        // BGM
        public static string BGMDirPath { get; } = "BGM/";

        // Text
        public static string TextDirPath { get; } = "Text/";

        // Log
        public static string LogDir { get; } = "Log/";
        public static string GameLogPath { get; } = "Log/game_log{0}.txt";
        public static string ErrorLogPath { get; } = "Log/error_log{0}.txt";
    }
}
