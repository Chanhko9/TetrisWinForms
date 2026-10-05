using System.Text.Json;
using TetrisWinForms.Models;

namespace TetrisWinForms.Managers;

public static class PreferencesManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static GamePreferences Current { get; private set; } = new();
    public static string SettingsFile => Path.Combine(AppPaths.DataDirectory, "settings.json");

    public static void Load()
    {
        try
        {
            AppPaths.EnsureFolders();
            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                Current = JsonSerializer.Deserialize<GamePreferences>(json) ?? new GamePreferences();
            }
        }
        catch
        {
            Current = new GamePreferences();
        }

        Current.MasterVolume = Math.Clamp(Current.MasterVolume, 0f, 1f);
        // These gameplay visuals are always enabled now that the Settings form was removed.
        Current.ScreenShakeEnabled = true;
        Current.GhostPieceEnabled = true;
        Current.ParticlesEnabled = true;
        AudioManager.Instance.SetMasterVolume(Current.MasterVolume);
    }

    public static void Save()
    {
        AppPaths.EnsureFolders();
        Current.MasterVolume = Math.Clamp(Current.MasterVolume, 0f, 1f);
        File.WriteAllText(SettingsFile, JsonSerializer.Serialize(Current, JsonOptions));
    }

    public static void SetMasterVolume(float value)
    {
        Current.MasterVolume = Math.Clamp(value, 0f, 1f);
        AudioManager.Instance.SetMasterVolume(Current.MasterVolume);
        Save();
    }
}
