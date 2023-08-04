namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using Terraria.GameInput;
    using Terraria.UI.Chat;
    using static Terraria.Main;
    using static MyGlobalItem;
    using Microsoft.Xna.Framework.Graphics;
    using Terraria.GameContent;
    using Terraria;
    using Terraria.ModLoader;

    class MyUIState : Terraria.UI.UIState
    {
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            int x = mouseX + (ThickMouse ? 16 : 10),
                y = mouseY + (ThickMouse ? 16 : 10);

            for(int i = 0, num = 20; i < 10; i++)
            {
                if(mouseX >= num && mouseX <= num + hotbarScale[i] * TextureAssets.InventoryBack.Width() && mouseY > (int)(19 + 22 * (1 - hotbarScale[i])) && mouseY < (int)(21 + 22 * (1 - hotbarScale[i])) + hotbarScale[i] * TextureAssets.InventoryBack.Height() && !(LocalPlayer.channel || LocalPlayer.ghost || LocalPlayer.hbLocked || LocalPlayer.inventory[i].type == 0 || PlayerInput.IgnoreMouseInterface || playerInventory))
                {
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, LocalPlayer.inventory[i].HoverName, new Vector2(x, y), TextPulse(RarityColor(LocalPlayer.inventory[i])), 0, Vector2.Zero, Vector2.One);

                    hoverItemName = "";
                }

                num += (int)(hotbarScale[i] * TextureAssets.InventoryBack.Width()) + 4;
            }

            for(int i = 400; i >= 0; i--)
            {

                Rectangle itemFrame = item[i].getRect();

                if(item[i].active && new Rectangle((int)item[i].position.X + item[i].width / 2 - itemFrame.Width / 2, (int)item[i].position.Y + item[i].height - itemFrame.Height, itemFrame.Width, itemFrame.Height).Contains((int)screenPosition.X + mouseX, (int)screenPosition.Y + mouseY) && !mouseText)
                {
                    Vector2 itemNameLength = FontAssets.MouseText.Value.MeasureString(item[i].HoverName);

                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, item[i].HoverName, new Vector2(x + itemNameLength.X + 4 > screenWidth ? screenWidth - itemNameLength.X - 4 : x, y + itemNameLength.Y + 4 > screenHeight ? screenHeight - itemNameLength.Y - 4 : y), TextPulse(RarityColor(item[i])), 0, Vector2.Zero, Vector2.One);

                    PlayerInput.SetZoom_Test();
                    PlayerInput.SetZoom_UI();

                    mouseText = true;
                }
            }
        }
    }
}