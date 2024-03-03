namespace TrueTooltips
{
    using Terraria;
    using Terraria.ModLoader;

    class MyMod : ModSystem
    {
        public override void Load()
        {
            Main.SettingsEnabled_OpaqueBoxBehindTooltips = false;
        }
        public override void Unload()
        {
            Main.SettingsEnabled_OpaqueBoxBehindTooltips = true;
        }

        public override void PostUpdateInput()
        {
            if (ModContent.GetInstance<Config>() != null)
                if (!ModContent.GetInstance<Config>().textPulse)
                {
                    Main.cursorAlpha = 0.6f;
                    Main.mouseTextColor = 255;
                }
        }

    }
}