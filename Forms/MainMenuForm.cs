using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class MainMenuForm : Form
{
    public MainMenuForm()
    {
        InitializeComponent();
        Resize += (_, _) => LayoutUi();
        Shown += (_, _) => AudioManager.Instance.PlayMusic("menu_theme.mp3");
    }

    private void OpenModeSelect()
    {
        using var select = new ModeSelectForm();
        Hide();
        select.ShowDialog(this);
        Show();
        AudioManager.Instance.PlayMusic("menu_theme.mp3");
        ActiveControl = null;
    }

    private void OpenScoreHistory()
    {
        using var form = new ScoreHistoryForm();
        form.ShowDialog(this);
        ActiveControl = null;
    }

    private void OpenHowToPlay()
    {
        using var form = new HowToPlayForm();
        form.ShowDialog(this);
        ActiveControl = null;
    }

    private void SetVolumeFromMouse(int x)
    {
        float ratio = Math.Clamp(x / (float)Math.Max(1, _volumeBar.Width), 0f, 1f);
        PreferencesManager.SetMasterVolume(ratio);
        RefreshVolumeUi();
    }

    private void AdjustVolume(float delta)
    {
        float next = Math.Clamp(PreferencesManager.Current.MasterVolume + delta, 0f, 1f);
        PreferencesManager.SetMasterVolume(next);
        RefreshVolumeUi();
    }

    private void RefreshVolumeUi()
    {
        float value = AudioManager.Instance.IsMuted ? 0f : PreferencesManager.Current.MasterVolume;
        _volumeFill.Width = Math.Max(0, (int)Math.Round(_volumeBar.Width * value));
        _volumeValue.Text = $"{(int)Math.Round(value * 100)}%";
        _volumeBar.Invalidate();
    }
}
