using TetrisWinForms.Game.SpecialItems;

namespace TetrisWinForms.Game;

public sealed record GameStepResult(
    bool PieceLocked = false,
    int LinesCleared = 0,
    int ScoreDelta = 0,
    bool GameOver = false,
    SpecialItemType? RelicGranted = null,
    int Combo = 0,
    bool BackToBackBonus = false,
    int ComboBonus = 0,
    int BackToBackBonusScore = 0);
