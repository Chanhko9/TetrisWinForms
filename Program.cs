using TetrisWinForms.Forms;
using TetrisWinForms.Managers;

namespace TetrisWinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            AppPaths.EnsureFolders();
            PreferencesManager.Load();
            Application.ApplicationExit += (_, _) => AudioManager.Instance.Dispose();
            Application.Run(new MainMenuForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The game encountered an unexpected error.\n\n{ex.Message}",
                "TETRIS - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
