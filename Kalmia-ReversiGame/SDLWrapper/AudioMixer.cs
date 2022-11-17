using System;
using System.Runtime.InteropServices;

namespace Kalmia_Game.SDLWrapper
{
    internal class MixerChunk : IDisposable
    {
        public IntPtr Ptr { get; }

        MixerChunk(IntPtr ptr)
        {
            this.Ptr = ptr;
        }

        ~MixerChunk() => Mix_FreeChunk(this.Ptr);

        public void Dispose()
        {
            Mix_FreeChunk(this.Ptr);
            GC.SuppressFinalize(this);
        }

        public static MixerChunk LoadWav(string filePath)
        {
            var ptr = Mix_LoadWAV(filePath);
            if (ptr == IntPtr.Zero)
                throw new SDLException($"Failed to open an audio file \"{filePath}\"");
            return new MixerChunk(ptr);
        }

        static IntPtr Mix_LoadWAV(string file)
        {
            return Mix_LoadWAV_RW(SDL_RWFromFile(file, "rb"), 1);
        }

        [DllImport(FilePath.SDL_MIXER_DLL_PATH, CharSet = CharSet.Ansi)]
        static extern IntPtr Mix_LoadWAV_RW(IntPtr src, int freesrc);

        [DllImport(FilePath.SDL_DLL_PATH, CharSet = CharSet.Ansi)]
        static extern IntPtr SDL_RWFromFile(string file, string mode);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern void Mix_FreeChunk(IntPtr chunck);
    }

    public class MixerMusic : IDisposable
    {
        public IntPtr Ptr { get; }

        MixerMusic(IntPtr ptr)
        {
            this.Ptr = ptr;
        }

        ~MixerMusic() => Mix_FreeMusic(this.Ptr);

        public void Dispose()
        {
            Mix_FreeMusic(this.Ptr);
            GC.SuppressFinalize(this);
        }

        public static MixerMusic LoadMusic(string filePath)
        {
            var ptr = Mix_LoadMUS(filePath);
            if (ptr == IntPtr.Zero)
                throw new SDLException($"Failed to open an audio file \"{filePath}\"");
            return new MixerMusic(ptr);
        }

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern IntPtr Mix_LoadMUS(string file);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern void Mix_FreeMusic(IntPtr music);
    }

    internal static class AudioMixer
    {
        const uint SDL_INIT_AUDIO = 0x00000010u;
        const int MIX_DEFAULT_FREQUENCY = 22050;
        const int AUDIO_S16SYS = 0x9010;
        const int MIX_DEFAULT_FORMAT = AUDIO_S16SYS;
        const int STEREO = 2;
        const int MIX_DEFAULT_CHUNCKSIZE = 4096;
        const int DEFAULT_MIXING_CHANNEL_NUM = 16;

        enum MixerInitFlag
        {
            MIX_INIT_FLAC = 0x00000001,
            MIX_INIT_MOD = 0x00000002,
            MIX_INIT_MP3 = 0x00000008,
            MIX_INIT_OGG = 0x00000010,
            MIX_INIT_MID = 0x00000020,
            MIX_INIT_OPUS = 0x00000040
        }

        public static void Init()
        {
            var err = SDL_Init(SDL_INIT_AUDIO);
            if (err != 0)
                throw new SDLException(err);
            if (Mix_OpenAudio(MIX_DEFAULT_FREQUENCY, MIX_DEFAULT_FORMAT, STEREO, MIX_DEFAULT_CHUNCKSIZE) != 0)
                throw new SDLException("Failed to open the audio device.");

            if (Mix_Init(MixerInitFlag.MIX_INIT_OGG) != MixerInitFlag.MIX_INIT_OGG)
                throw new SDLException("Failed to initialize SDL mixer.");
            Mix_AllocateChannels(DEFAULT_MIXING_CHANNEL_NUM);
        }

        public static void Quit()
        {
            Mix_CloseAudio();
            Mix_Quit();
            SDL_Quit();
        }

        /// <summary>
        /// 指定された数だけミキシングチャンネルを確保します.
        /// </summary>
        /// <param name="channelNum">確保するチャンネル数. -1を指定した時は現在確保されているチャンネル数を調べる.</param>
        /// <returns>確保したチャンネルの数</returns>
        public static int AllocateChannels(int channelNum)
        {
            return Mix_AllocateChannels(channelNum);
        }

        /// <summary>
        /// 指定されたチャンネルでオーディオを再生します.
        /// </summary>
        /// <param name="channel">再生するチャンネル. -1のとき予約されていない最初の空きチャンネル.</param>
        /// <param name="chunck">再生する音声</param>
        /// <param name="loops">ループ回数. -1のとき無限ループ.</param>
        /// <returns>再生に使用したチャンネル. 再生に失敗した場合は-1</returns>
        public static int PlayChannel(int channel, MixerChunk chunck, int loops = 0)
        {
            return Mix_PlayChannel(channel, chunck.Ptr, loops);
        }

        /// <summary>
        /// 指定されたチャンネルで再生されているオーディオを停止します.
        /// </summary>
        /// <param name="channel">停止するチャンネル. -1の場合は全てのチャンネルを停止.</param>
        public static void HaltChannel(int channel = -1)
        {
            Mix_HaltChannel(channel);
        }

        /// <summary>
        /// 指定されたチャンネルが使用中かを確認します.
        /// </summary>
        /// <param name="channel">確認するチャンネル. -1を指定した場合は全てのチャンネルを調べる.</param>
        /// <returns>指定されたチャンネルが使用されていればtrue. channelに-1を指定した場合は1つでも使用されているチャンネルが存在すればtrue.</returns>
        public static bool Playing(int channel)
        {
            return Mix_Playing(channel) != 0;
        }

        /// <summary>
        /// 指定されたチャンネルの音量を設定します.
        /// </summary>
        /// <param name="channel">音量を設定するチャンネル. -1を指定した場合は全てのチャンネルに適用する.</param>
        /// <param name="volume">音量 0 ~ 128の値</param>
        /// <returns>現在のチャンネルの音量. channelに-1を指定した場合は全てのチャンネルの音量の平均値.</returns>
        public static int SetChannelVolume(int channel, int volume)
        {
            return Mix_Volume(channel, volume);
        }

        /// <summary>
        /// 音楽を再生します.
        /// </summary>
        /// <param name="music">再生する音楽</param>
        /// <param name="loops">再生回数. -1のとき無限回再生.</param>
        public static void PlayMusic(MixerMusic music, int loops = -1)
        {
            if (Mix_PlayMusic(music.Ptr, loops) == -1)
                throw new SDLException("Failed to play music.");
        }

        /// <summary>
        /// 再生中の音楽を停止します.
        /// </summary>
        public static void HaltMusic()
        {
            Mix_HaltMusic();
        }

        /// <summary>
        /// 音楽を指定されたミリ秒でフェードインさせます.
        /// </summary>
        /// <param name="music">フェードインさせる音楽</param>
        /// <param name="loops">再生回数. -1のとき無限回再生</param>
        /// <param name="ms">フェードインが完了する時間</param>
        public static void FadeInMusic(MixerMusic music, int loops, int ms)
        {
            if (Mix_FadeInMusic(music.Ptr, loops, ms) == -1)
                throw new SDLException("Failed to fade in music.");
        }

        /// <summary>
        /// 再生中の音楽を指定されたミリ秒でフェードアウトさせます.
        /// </summary>
        /// <param name="ms">フェードアウトが完了する時間</param>
        public static void FadeOutMusic(int ms)
        {
            if (Mix_FadeOutMusic(ms) == -1)
                throw new SDLException("Failed to fade out music.");
        }

        /// <summary>
        /// 音楽が再生中か確認します.
        /// </summary>
        /// <returns>再生中ならtrue</returns>
        public static bool PlayingMusic()
        {
            return Mix_PlayingMusic() != 0;
        }

        /// <summary>
        /// 音楽の音量を設定します.
        /// </summary>
        /// <param name="volume">音量 0 ~ 128の値. -1を指定した場合は現在の音量を確認する.</param>
        /// <returns>設定前の音量. volumeに-1が指定された場合は現在の音量.</returns>
        public static int SetMusicVolume(int volume)
        {
            return Mix_VolumeMusic(volume);
        }

        static int Mix_PlayChannel(int channel, IntPtr chunk, int loops)
        {
            return Mix_PlayChannelTimed(channel, chunk, loops, -1);
        }

        [DllImport(FilePath.SDL_DLL_PATH)]
        static extern int SDL_Init(uint flags);

        [DllImport(FilePath.SDL_DLL_PATH)]
        static extern void SDL_Quit();

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern MixerInitFlag Mix_Init(MixerInitFlag flags);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern void Mix_Quit();

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_OpenAudio(int frequency, ushort format, int channels, int chunksize);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern void Mix_CloseAudio();

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_AllocateChannels(int numchans);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_PlayChannelTimed(int channel, IntPtr chunk, int loops, int ticks);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_HaltChannel(int channel);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_Playing(int channel);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_Volume(int channel, int volume);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_PlayMusic(IntPtr music, int loops);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_HaltMusic();

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_FadeInMusic(IntPtr music, int loops, int ms);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_FadeOutMusic(int ms);

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_PlayingMusic();

        [DllImport(FilePath.SDL_MIXER_DLL_PATH)]
        static extern int Mix_VolumeMusic(int volume);
    }
}
