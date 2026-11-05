using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace TrueTooltips;

/// <summary>
/// The version layer: anything that can't compile on both 1.4.4 and 1.4.5 goes here, so all other files stay identical.
/// Mods are built separately per Terraria version, so the 1.4.4 and 1.4.5 git branches each have their own copy of this file.
/// </summary>
internal static class TooltipLineCompat
{
    public static void SetColor(this TooltipLine line, Color color) => line.OverrideColor = color;

    public static bool IsGoodPrefix(this TooltipLine line) => line.IsModifier && !line.IsModifierBad;

    public static bool IsBadPrefix(this TooltipLine line) => line.IsModifier && line.IsModifierBad;
}
