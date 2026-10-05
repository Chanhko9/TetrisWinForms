using TetrisWinForms.Core;
using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class ModeSelectForm : Form
{
    public ModeSelectForm()
    {
        InitializeComponent();
        Shown += (_, _) => AudioManager.Instance.PlayMusic("menu_theme.mp3");
        Resize += (_, _) => LayoutUi();
    }

    private void Launch(GameMode mode)
    {
        AudioManager.Instance.StopMusic();
        Hide();

        if (mode == GameMode.DuelArena)
        {
            using var duel = new DuelArenaForm();
            duel.ShowDialog(this);
        }
        else
        {
            using var game = new GameForm(mode);
            game.ShowDialog(this);
        }

        Show();
        AudioManager.Instance.PlayMusic("menu_theme.mp3");
    }
}
