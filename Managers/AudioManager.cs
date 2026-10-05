using NAudio.Wave;

namespace TetrisWinForms.Managers;

public sealed class AudioManager : IDisposable
{
    public static AudioManager Instance { get; } = new();

    private WaveOut? _musicOutput;
    private AudioFileReader? _musicReader;
    private bool _loopMusic;
    private readonly List<(WaveOut output, AudioFileReader reader)> _activeSfx = new();
    private readonly object _sfxLock = new();

    public float MusicVolume { get; private set; } = 0.65f;
    public float SfxVolume { get; private set; } = 0.80f;
    public bool IsMuted { get; private set; }

    private AudioManager() { }

    public void PlayMusic(string fileName, bool loop = true)
    {
        string path = AppPaths.Audio(fileName);
        if (!File.Exists(path)) return;

        StopMusic();
        _loopMusic = loop;

        _musicReader = new AudioFileReader(path)
        {
            Volume = IsMuted ? 0f : MusicVolume
        };

        _musicOutput = new WaveOut();
        _musicOutput.Init(_musicReader);
        _musicOutput.PlaybackStopped += MusicOutput_PlaybackStopped;
        _musicOutput.Play();
    }

    private void MusicOutput_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (!_loopMusic || _musicReader is null || _musicOutput is null) return;
        if (e.Exception is not null) return;

        _musicReader.Position = 0;
        _musicOutput.Play();
    }

    public void StopMusic()
    {
        _loopMusic = false;

        if (_musicOutput is not null)
        {
            _musicOutput.PlaybackStopped -= MusicOutput_PlaybackStopped;
            _musicOutput.Stop();
            _musicOutput.Dispose();
            _musicOutput = null;
        }

        _musicReader?.Dispose();
        _musicReader = null;
    }

    public void PlaySfx(string fileName, float volumeScale = 1f)
    {
        string path = AppPaths.Audio("Sfx", fileName);
        if (!File.Exists(path)) return;

        var reader = new AudioFileReader(path)
        {
            Volume = IsMuted ? 0f : Math.Clamp(SfxVolume * volumeScale, 0f, 1f)
        };
        var output = new WaveOut();
        output.Init(reader);

        lock (_sfxLock) _activeSfx.Add((output, reader));
        output.PlaybackStopped += (_, _) =>
        {
            output.Dispose();
            reader.Dispose();
            lock (_sfxLock) _activeSfx.RemoveAll(x => ReferenceEquals(x.output, output));
        };
        output.Play();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Math.Clamp(value, 0f, 1f);
        if (_musicReader is not null)
            _musicReader.Volume = IsMuted ? 0f : MusicVolume;
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Math.Clamp(value, 0f, 1f);
    }

    public void SetMasterVolume(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        SetMusicVolume(value);
        SetSfxVolume(value);
    }

    public float ChangeMasterVolume(float delta)
    {
        float value = Math.Clamp(MusicVolume + delta, 0f, 1f);
        SetMasterVolume(value);
        return value;
    }

    public float ChangeMusicVolume(float delta)
    {
        SetMusicVolume(MusicVolume + delta);
        return MusicVolume;
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        if (_musicReader is not null)
            _musicReader.Volume = IsMuted ? 0f : MusicVolume;
    }

    public void Dispose()
    {
        StopMusic();
        lock (_sfxLock)
        {
            foreach (var (output, reader) in _activeSfx.ToArray())
            {
                output.Dispose();
                reader.Dispose();
            }
            _activeSfx.Clear();
        }
    }
}
