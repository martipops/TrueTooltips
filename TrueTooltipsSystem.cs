using Terraria;
using Terraria.ModLoader;

namespace TrueTooltips;

internal class TrueTooltipsSystem : ModSystem
{
    public override void Load()
    {
        // The mod draws its own background.
        Main.SettingsEnabled_OpaqueBoxBehindTooltips = false;
    }

    public override void Unload()
    {
        Main.SettingsEnabled_OpaqueBoxBehindTooltips = true;
    }

    public override void PostUpdateInput()
    {
        // The config can be null while the mod is loading or unloading.
        if (ModContent.GetInstance<Config>() is { textPulse: false })
        {
            Main.cursorAlpha = 0.6f;
            Main.mouseTextColor = 255;
        }
    }
}
