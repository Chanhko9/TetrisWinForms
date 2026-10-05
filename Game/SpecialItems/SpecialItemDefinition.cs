using TetrisWinForms.Effects;

namespace TetrisWinForms.Game.SpecialItems;

public sealed record SpecialItemDefinition(
    SpecialItemType Type,
    string DisplayName,
    string Description,
    string SfxFile,
    int ShakeIntensity,
    int ShakeDurationMs,
    ShakeAxis ShakeAxis)
{
    public static readonly IReadOnlyDictionary<SpecialItemType, SpecialItemDefinition> All =
        new Dictionary<SpecialItemType, SpecialItemDefinition>
        {
            [SpecialItemType.RuneBomb] = new(
                SpecialItemType.RuneBomb, "Rune Bomb", "Destroy a 3×3 area", "rune_bomb.wav", 7, 220, ShakeAxis.Both),

            [SpecialItemType.DragonBreath] = new(
                SpecialItemType.DragonBreath, "Dragon Breath", "Burn away one full row", "dragon_breath.wav", 5, 180, ShakeAxis.Horizontal),

            [SpecialItemType.ThunderSpear] = new(
                SpecialItemType.ThunderSpear, "Thunder Spear", "Strike through one full column", "thunder_spear.wav", 6, 190, ShakeAxis.Vertical),

            [SpecialItemType.VoidCross] = new(
                SpecialItemType.VoidCross, "Void Cross", "Destroy a plus-shaped area", "void_cross.wav", 8, 260, ShakeAxis.Both),

            [SpecialItemType.PrismRelic] = new(
                SpecialItemType.PrismRelic, "Prism Relic", "Remove every block of one color", "prism_relic.wav", 4, 160, ShakeAxis.Both),

            [SpecialItemType.MeteorRelic] = new(
                SpecialItemType.MeteorRelic, "Meteor Relic", "Smash a 4×4 impact zone", "meteor.wav", 11, 360, ShakeAxis.Both),

            [SpecialItemType.BlackHole] = new(
                SpecialItemType.BlackHole, "Black Hole", "Pull out a diamond-shaped area", "black_hole.wav", 12, 420, ShakeAxis.Both),

            [SpecialItemType.PhoenixSigil] = new(
                SpecialItemType.PhoenixSigil, "Phoenix Sigil", "Burn both diagonals in an X shape", "phoenix.wav", 9, 300, ShakeAxis.Both),

            [SpecialItemType.ChaosDice] = new(
                SpecialItemType.ChaosDice, "Chaos Dice", "Randomly destroy up to 18 blocks", "chaos_dice.wav", 8, 280, ShakeAxis.Both)
        };
}
