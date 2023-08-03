﻿namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using Terraria;
    using Terraria.UI;
    using Terraria.ModLoader;

    class MyMod : ModSystem
    {
        readonly UserInterface ui = new UserInterface();

        public override void Load()
        {
            ui.SetState(new MyUIState());
        }
        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)/* tModPorter Note: Removed. Use ModSystem.ModifyInterfaceLayers */
        {
            int mouseText = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");

            if(mouseText >= 0)
            {
                layers.Insert(mouseText, new LegacyGameInterfaceLayer("", () =>
                {
                    ui.Draw(Main.spriteBatch, new GameTime());

                    return true;
                }, InterfaceScaleType.UI));
            }
        }

        public override void UpdateUI(GameTime gameTime)/* tModPorter Note: Removed. Use ModSystem.UpdateUI */ => ui?.Update(gameTime);

        public override void PostUpdateInput()/* tModPorter Note: Removed. Use ModSystem.PostUpdateInput */
        {
            if(Terraria.ModLoader.ModContent.GetInstance<Config>() != null)
                if(!Terraria.ModLoader.ModContent.GetInstance<Config>().textPulse)
                {
                    Main.cursorAlpha = 0.6f;
                    Main.mouseTextColor = 255;
                }
        }
    }
}