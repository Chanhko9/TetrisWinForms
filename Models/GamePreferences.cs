namespace TetrisWinForms.Models;

public sealed class GamePreferences
{
    public float MasterVolume { get; set; } = 0.65f;
    public bool ScreenShakeEnabled { get; set; } = true;
    public bool GhostPieceEnabled { get; set; } = true;
    public bool ParticlesEnabled { get; set; } = true;
}
