namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Terraria;
    using Terraria.GameContent;
    using Terraria.GameContent.UI;
    using Terraria.ID;
    using Terraria.ModLoader;
    using Terraria.UI.Chat;
    using Terraria.Localization;
    using Microsoft.Xna.Framework.Graphics;

    class MyGlobalItem : GlobalItem
    {

        static readonly Config config = ModContent.GetInstance<Config>();

        static readonly string[] names = { "Ammo", "AmmoLine", "AxePower", "BaitPower", "Consumable", "CritChance", "Damage", "Defense", "Equipable", "FishingPower", "HammerPower", "HealLife", "HealMana", "ItemName", "Knockback", "Material", "PickPower", "Placeable", "PriceLine", "Speed", "TileBoost", "UseMana", "Velocity" };


        static readonly Dictionary<int, string> AmmoTypeNames = new Dictionary<int, string>()
        {
            [40] = "Mods.TrueTooltips.Configs.Config.ammoLine.Arrow",
            [71] = "Mods.TrueTooltips.Configs.Config.ammoLine.Coin",
            [97] = "Mods.TrueTooltips.Configs.Config.ammoLine.Bullet",
            [169] = "Mods.TrueTooltips.Configs.Config.ammoLine.Sand",
            [283] = "Mods.TrueTooltips.Configs.Config.ammoLine.Dart",
            [771] = "Mods.TrueTooltips.Configs.Config.ammoLine.Rocket",
            [780] = "Mods.TrueTooltips.Configs.Config.ammoLine.Solution",
            [931] = "Mods.TrueTooltips.Configs.Config.ammoLine.Flare"
        };

        static readonly Regex specialPriceRegex = new Regex($@"{Language.GetTextValue("LegacyTooltip.50")}\s");
        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int _x, ref int _y)
        {
            Texture2D texture = TextureAssets.Item[item.type].Value;
            Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();

            var (width, textHeight) = MeasureTooltipLines(lines);

            int spriteSize = config.sprite ? new[] { frame.Width, frame.Height, config.spriteMin }.Max() : 0;
            int borderPadding = config.sprite && config.spriteBorder ? config.spriteBorderPadding : 0;
            int spriteOffsetX = config.sprite ? spriteSize + config.spriteTextPadding : 0;
            int minSpriteHeight = config.sprite ? spriteSize + borderPadding * 2 : 0;
            int height = Math.Max(textHeight, minSpriteHeight);

            int totalWidth = width + config.paddingLeft + config.paddingRight + spriteOffsetX + borderPadding;
            int totalHeight = height + config.paddingTop + config.paddingBottom + borderPadding;

            int x = _x, y = _y;
            ClampToScreen(ref x, totalWidth, Main.screenWidth, config.paddingLeft);
            ClampToScreen(ref y, totalHeight, Main.screenHeight, config.paddingTop);
            _x = x;
            _y = y;

            // apply configured offset
            _x += config.paddingLeft + config.x;
            _y += config.paddingTop + config.y;
            x += config.x;
            y += config.y;

            int bgX = x, bgY = y;
            int bgWidth = width + config.paddingLeft + config.paddingRight;
            int bgHeight = height + config.paddingTop + config.paddingBottom;

            if (config.sprite)
            {
                _x += config.spriteTextPadding + spriteOffsetX;
                bgWidth += config.spriteTextPadding + spriteOffsetX;

                if (config.spriteBorder)
                {
                    bgWidth += borderPadding;
                    _x += borderPadding;
                }

                DrawTooltipBG(bgX, bgY, bgWidth, bgHeight, config.bgColor, config.borderColor);

                if (config.spriteBorder)
                {
                    int borderX = x + config.paddingLeft;
                    int borderY = y + config.paddingTop;
                    int borderSide = spriteSize + borderPadding * 2;
                    DrawTooltipBG(borderX, borderY, borderSide, borderSide, config.spritebgColor, config.borderColor);
                }

                int spriteX = x + (spriteSize - frame.Width) / 2 + borderPadding + config.paddingLeft;
                int spriteY = y + (spriteSize - frame.Height) / 2 + borderPadding + config.paddingTop;
                Main.spriteBatch.Draw(texture, new Vector2(spriteX, spriteY), frame, Color.White);
            }
            else
            {
                DrawTooltipBG(bgX, bgY, bgWidth, bgHeight, config.bgColor, config.borderColor);
            }

            return true;
        }

        private Dictionary<string, TooltipLine> GetTooltipLineCache(List<TooltipLine> lines)
        {
            Dictionary<string, TooltipLine> cache = new Dictionary<string, TooltipLine>(40);
            foreach (var line in lines)
            {
                if (!cache.ContainsKey(line.Name))
                    cache[line.Name] = line;
            }
            return cache;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> lines)
        {
            Item currentAmmo = null;

            if (config.ammoLine)
            {
                if (item.fishingPole > 0)
                {
                    // Find bait for fishing poles
                    foreach (Item invItem in Main.LocalPlayer.inventory)
                    {
                        if (invItem.active && invItem.bait > 0)
                        {
                            currentAmmo = invItem;
                            break;
                        }
                    }
                }
                else if (item.useAmmo > 0)
                {
                    // Find ammo for weapons
                    foreach (Item invItem in Main.LocalPlayer.inventory)
                    {
                        if (invItem.active && invItem.ammo == item.useAmmo)
                        {
                            currentAmmo = invItem;
                            break;
                        }
                    }
                }
                else if (item.tileWand > 0)
                {
                    // Find tiles for wands
                    foreach (Item invItem in Main.LocalPlayer.inventory)
                    {
                        if (invItem.active && invItem.type == item.tileWand)
                        {
                            currentAmmo = invItem;
                            break;
                        }
                    }
                }
            }

            if (item.type == ItemID.CoinGun)
            {
                int coinGunCrit = Main.LocalPlayer.GetWeaponCrit(item);
                lines.InsertRange(1, new[] {
                    new TooltipLine(Mod, "Damage", "0" + Language.GetTextValue("LegacyTooltip.3")),
                    new TooltipLine(Mod, "CritChance", coinGunCrit + Language.GetTextValue("LegacyTooltip.41")),
                    new TooltipLine(Mod, "Speed", ""),
                    new TooltipLine(Mod, "Knockback", "")
                });
            }

            // Get tooltip line cache for faster lookups
            var lineCache = GetTooltipLineCache(lines);

            TooltipLine ammoLine = new(Mod, "AmmoLine", currentAmmo?.HoverName);

            lineCache.TryGetValue("Ammo", out var ammo);
            lineCache.TryGetValue("AxePower", out var axePow);
            lineCache.TryGetValue("BaitPower", out var baitPow);
            lineCache.TryGetValue("BuffTime", out var buffTime);
            lineCache.TryGetValue("Consumable", out var consumable);
            lineCache.TryGetValue("CritChance", out var critChance);
            lineCache.TryGetValue("Defense", out var defense);
            lineCache.TryGetValue("Damage", out var dmg);
            lineCache.TryGetValue("Equipable", out var equipable);
            lineCache.TryGetValue("EtherianManaWarning", out var etherianMana);
            lineCache.TryGetValue("Expert", out var expert);
            lineCache.TryGetValue("Favorite", out var fav);
            lineCache.TryGetValue("FavoriteDesc", out var favDescr);
            lineCache.TryGetValue("FishingPower", out var fishingPow);
            lineCache.TryGetValue("HammerPower", out var hammerPow);
            lineCache.TryGetValue("HealLife", out var healLife);
            lineCache.TryGetValue("HealMana", out var healMana);
            lineCache.TryGetValue("Knockback", out var knockback);
            lineCache.TryGetValue("Material", out var material);
            lineCache.TryGetValue("ItemName", out var name);
            lineCache.TryGetValue("NeedsBait", out var needsBait);
            lineCache.TryGetValue("PickPower", out var pickPow);
            lineCache.TryGetValue("Placeable", out var placeable);
            lineCache.TryGetValue("Quest", out var quest);
            lineCache.TryGetValue("SetBonus", out var setBonus);
            lineCache.TryGetValue("Social", out var social);
            lineCache.TryGetValue("SocialDesc", out var socialDescr);
            lineCache.TryGetValue("Speed", out var speed);
            lineCache.TryGetValue("TileBoost", out var tileBoost);
            lineCache.TryGetValue("UseMana", out var useMana);
            lineCache.TryGetValue("Vanity", out var vanity);
            lineCache.TryGetValue("WandConsumes", out var wandConsumes);
            lineCache.TryGetValue("WellFedExpert", out var wellFedExpert);
            lineCache.TryGetValue("Price", out var price);
            lineCache.TryGetValue("SpecialPrice", out var specialPrice);
            lineCache.TryGetValue("JourneyResearch", out var journeyResearch);

            if (config.velocityLine.A > 0 && item.shootSpeed > 0)
            {
                TooltipLine velocityLine = new TooltipLine(Mod, "Velocity", item.shootSpeed + (currentAmmo != null && config.wpnPlusAmmoVelocity ? currentAmmo.shootSpeed : 0) + Language.GetTextValue("LegacyTooltip.44").Substring(1)) { OverrideColor = config.velocityLine };
                lines.Insert(lines.IndexOf(knockback ?? speed ?? critChance ?? dmg ?? equipable ?? name) + 1, velocityLine);
            }

            int index = lines.FindLastIndex(l => names.Contains(l.Name)) + 1;

            if (item.IsACoin)
            {
                if (!lines.Contains(dmg))
                {
                    lines.Insert(1, new TooltipLine(Mod, "Damage", Main.LocalPlayer.GetWeaponDamage(item) + " " + Language.GetTextValue("LegacyTooltip.3")));
                    dmg = lines.Find(l => l.Name == "Damage");
                }

                lines.Insert(lines.IndexOf(lines.Find(l => l.Name == "Velocity") ?? dmg ?? name) + 1, new TooltipLine(Mod, "Ammo", Language.GetTextValue("LegacyTooltip.34")));
                lines.Add(new TooltipLine(Mod, "Material", Language.GetTextValue("LegacyTooltip.36")));

                ammo = lines.Find(l => l.Name == "Ammo");
                material = lines.Find(l => l.Name == "Material");
            }

            if (item.wingSlot > 0 && config.wingFlyTimeLine)
            {
                string unit = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.wingFlyTimeLine.Unit");
                int wingTime = ArmorIDs.Wing.Sets.Stats[item.wingSlot].FlyTime;
                lines.Insert(
                    lines.IndexOf(lines.Find(l => l.Name == "Equipable") ?? name) + 1,
                    new TooltipLine(Mod, "WingTime", string.Format(unit, wingTime))
                );
                index++;
            }

            if (config.priceLine)
            {
                long priceOfStack = GetAdjustedPrice(item);
                if (priceOfStack > 0 && !item.IsACoin)
                {
                    string priceText = "";
                    if (item.shopSpecialCurrency >= 0)
                    {
                        priceText += item.buy ?
                            specialPriceRegex.Replace(
                                lines.Find(l => l.Name == "SpecialPrice").Text ?? "", "", 1) : "";
                    }
                    else
                    {
                        long plat = priceOfStack / 1_000_000,
                            gold = priceOfStack / 10000 % 100,
                            silver = priceOfStack / 100 % 100,
                            copper = priceOfStack % 100;
                        if (plat > 0)
                            priceText += "[c/" + TextPulse(Colors.CoinPlatinum).Hex3() + ":" + plat + " " + Lang.inter[15].Value + " ]";
                        if (gold > 0)
                            priceText += "[c/" + TextPulse(Colors.CoinGold).Hex3() + ":" + gold + " " + Lang.inter[16].Value + " ]";
                        if (silver > 0)
                            priceText += "[c/" + TextPulse(Colors.CoinSilver).Hex3() + ":" + silver + " " + Lang.inter[17].Value + " ]";
                        if (copper > 0)
                            priceText += "[c/" + TextPulse(Colors.CoinCopper).Hex3() + ":" + copper + " " + Lang.inter[18].Value + " ]";
                    }
                    lines.Add(new TooltipLine(Mod, "PriceLine", priceText));
                }

                price?.Hide();
                specialPrice?.Hide();
                // lines.FindAll(l => l.Name == "Price" || l.Name == "SpecialPrice").ForEach(line => line?.Hide());
            }

            if (config.ammoLine)
            {
                if (currentAmmo != null)
                    lines.Insert(index, ammoLine);
                else if (item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
                {
                    string ammoName;
                    if (item.fishingPole > 0)
                    {
                        ammoName = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Bait");
                    }
                    else if (AmmoTypeNames.TryGetValue(item.useAmmo, out string value))
                    {
                        ammoName = Language.GetTextValue(value);
                    }
                    else
                    {
                        ammoName = Lang.GetItemNameValue(item.useAmmo > 0 ? item.useAmmo : item.tileWand);
                    }

                    lines.Insert(index, new TooltipLine(Mod, "AmmoLine",
                        Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.No") + ammoName)
                    {
                        OverrideColor = RarityColor(currentAmmo)
                    });
                }
                needsBait?.Hide();
                wandConsumes?.Hide();
            }

            if (config.modNameNextToItemName)
            {
                if (currentAmmo?.ModItem != null)
                    ammoLine.Text += " - " + currentAmmo.ModItem.Mod.DisplayName;

                if (item.ModItem != null)
                    name.Text += " - " + item.ModItem.Mod.DisplayName;
            }
            else if (config.modNameColor.A != 0 && item.ModItem != null)
            {
                lines.Add(new TooltipLine(Mod, "ModName", item.ModItem.Mod.DisplayName) { OverrideColor = config.modNameColor });
            }

            if (ammo != null && !config.ammo.Equals(Color.White)) ammo.OverrideColor = config.ammo;
            if (axePow != null && !config.axePow.Equals(Color.White)) axePow.OverrideColor = config.axePow;
            if (baitPow != null && !config.baitPow.Equals(Color.White)) baitPow.OverrideColor = config.baitPow;
            if (buffTime != null && !config.buffTime.Equals(Color.White)) buffTime.OverrideColor = config.buffTime;
            if (consumable != null && !config.consumable.Equals(Color.White)) consumable.OverrideColor = config.consumable;
            if (critChance != null && !config.critChance.Equals(Color.White)) critChance.OverrideColor = config.critChance;
            if (defense != null && !config.defense.Equals(Color.White)) defense.OverrideColor = config.defense;

            if (dmg != null)
            {
                if (config.wpnPlusAmmoDmg && currentAmmo != null)
                    dmg.Text = dmg.Text.Replace(dmg.Text.Split(' ').First(), Main.LocalPlayer.GetWeaponDamage(item) + Main.LocalPlayer.GetWeaponDamage(currentAmmo) + "");
                if (!config.dmg.Equals(Color.White)) dmg.OverrideColor = config.dmg;
            }

            if (equipable != null && !config.equipable.Equals(Color.White)) equipable.OverrideColor = config.equipable;
            if (etherianMana != null && !config.etherianMana.Equals(Color.White)) etherianMana.OverrideColor = config.etherianMana;
            if (expert != null && !config.expert.Equals(Color.White)) expert.OverrideColor = config.expert;
            if (fav != null && !config.fav.Equals(Color.White)) fav.OverrideColor = config.fav;
            if (favDescr != null && !config.favDescr.Equals(Color.White)) favDescr.OverrideColor = config.favDescr;
            if (fishingPow != null && !config.fishingPow.Equals(Color.White)) fishingPow.OverrideColor = config.fishingPow;
            if (hammerPow != null && !config.hammerPow.Equals(Color.White)) hammerPow.OverrideColor = config.hammerPow;
            if (healLife != null && !config.healLife.Equals(Color.White)) healLife.OverrideColor = config.healLife;
            if (healMana != null && !config.healMana.Equals(Color.White)) healMana.OverrideColor = config.healMana;
            if (journeyResearch != null && !config.journeyResearch.Equals(Color.White)) journeyResearch.OverrideColor = config.journeyResearch;

            if (knockback != null && config.knockbackLine)
            {
                float kbScale = 1f;
                if (item.CountsAsClass(DamageClass.Melee) && Main.LocalPlayer.kbGlove)
                    kbScale += 1f;

                if (Main.LocalPlayer.kbBuff)
                    kbScale += 0.5f;

                if (kbScale != 1f)
                    item.knockBack *= kbScale;

                if (item.CountsAsClass(DamageClass.Ranged) && Main.LocalPlayer.shroomiteStealth)
                    item.knockBack *= 1f + (1f - Main.LocalPlayer.stealth) * 0.5f;

                knockback.Text = Math.Round(item.knockBack + (currentAmmo != null && config.wpnPlusAmmoKb ? Main.LocalPlayer.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0), 2) + Language.GetTextValue("LegacyTooltip.45").Substring(1);

                if (!config.knockback.Equals(Color.White)) knockback.OverrideColor = config.knockback;

            }

            if (material != null && !config.material.Equals(Color.White)) material.OverrideColor = config.material;
            if (needsBait != null && !config.needsBait.Equals(Color.White)) needsBait.OverrideColor = config.needsBait;
            if (pickPow != null && !config.pickPow.Equals(Color.White)) pickPow.OverrideColor = config.pickPow;
            if (placeable != null && !config.placeable.Equals(Color.White)) placeable.OverrideColor = config.placeable;
            if (quest != null && !config.quest.Equals(Color.White)) quest.OverrideColor = config.quest;
            if (setBonus != null && !config.setBonus.Equals(Color.White)) setBonus.OverrideColor = config.setBonus;
            if (social != null && !config.social.Equals(Color.White)) social.OverrideColor = config.social;
            if (socialDescr != null && !config.socialDescr.Equals(Color.White)) socialDescr.OverrideColor = config.socialDescr;

            if (speed != null)
            {
                if (config.speedLine)
                {
                    float attackSpeedModifier = item.CountsAsClass(DamageClass.Melee) ? Main.LocalPlayer.GetAttackSpeed(DamageClass.Melee) : 1f;
                    float useTime = item.useAnimation / attackSpeedModifier;
                    float totalDelay = item.reuseDelay > 0 ? item.reuseDelay : useTime;
                    speed.Text = Math.Round(60f / totalDelay, 2) + Language.GetTextValue("Mods.TrueTooltips.Configs.Config.speedLine.Display");
                }
                if (!config.speed.Equals(Color.White)) speed.OverrideColor = config.speed;
            }

            if (tileBoost != null && !config.tileBoost.Equals(Color.White)) tileBoost.OverrideColor = config.tileBoost;
            if (useMana != null && !config.useMana.Equals(Color.White)) useMana.OverrideColor = config.useMana;
            if (vanity != null && !config.vanity.Equals(Color.White)) vanity.OverrideColor = config.vanity;
            if (wandConsumes != null && !config.wandConsumes.Equals(Color.White)) wandConsumes.OverrideColor = config.wandConsumes;
            if (wellFedExpert != null && !config.wellFedExpert.Equals(Color.White)) wellFedExpert.OverrideColor = config.wellFedExpert;

            foreach (TooltipLine line in lines)
            {
                if (line.IsModifier && !config.badMod.Equals(Color.White))
                    line.OverrideColor = line.IsModifierBad ? config.badMod : config.goodMod;
            }

            if (config.ammo.A == 0) ammo?.Hide();
            if (config.axePow.A == 0) axePow?.Hide();
            if (config.baitPow.A == 0) baitPow?.Hide();
            if (config.buffTime.A == 0) buffTime?.Hide();
            if (config.consumable.A == 0) consumable?.Hide();
            if (config.critChance.A == 0) critChance?.Hide();
            if (config.defense.A == 0) defense?.Hide();
            if (config.dmg.A == 0) dmg?.Hide();
            if (config.equipable.A == 0) equipable?.Hide();
            if (config.etherianMana.A == 0) etherianMana?.Hide();
            if (config.expert.A == 0) expert?.Hide();
            if (config.fav.A == 0) fav?.Hide();
            if (config.favDescr.A == 0) favDescr?.Hide();
            if (config.fishingPow.A == 0) fishingPow?.Hide();
            if (config.hammerPow.A == 0) hammerPow?.Hide();
            if (config.healLife.A == 0) healLife?.Hide();
            if (config.healMana.A == 0) healMana?.Hide();
            if (config.knockback.A == 0) knockback?.Hide();
            if (config.material.A == 0) material?.Hide();
            if (config.needsBait.A == 0) needsBait?.Hide();
            if (config.pickPow.A == 0) pickPow?.Hide();
            if (config.placeable.A == 0) placeable?.Hide();
            if (config.quest.A == 0) quest?.Hide();
            if (config.setBonus.A == 0) setBonus?.Hide();
            if (config.social.A == 0) social?.Hide();
            if (config.socialDescr.A == 0) socialDescr?.Hide();
            if (config.speed.A == 0) speed?.Hide();
            if (config.tileBoost.A == 0) tileBoost?.Hide();
            if (config.useMana.A == 0) useMana?.Hide();
            if (config.vanity.A == 0) vanity?.Hide();
            if (config.wandConsumes.A == 0) wandConsumes?.Hide();
            if (config.wellFedExpert.A == 0) wellFedExpert?.Hide();
            if (config.journeyResearch.A == 0) journeyResearch?.Hide();
            if (config.badMod.A == 0 || config.goodMod.A == 0)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (line.IsModifierBad && config.badMod.A == 0)
                    {
                        line.Hide();
                    }
                    else if (line.IsModifier && config.goodMod.A == 0)
                    {
                        line.Hide();
                    }
                }
            }
        }

        private static long GetAdjustedPrice(Item item)
        {
            if (item.value == 0) return 0;
            Main.LocalPlayer.GetItemExpectedPrice(item, out var calcForSelling, out var calcForBuying);
            long itemPrice = (item.isAShopItem || item.buyOnce) ? calcForBuying : calcForSelling;
            long priceOfStack;
            if (item.buy)
            {
                priceOfStack = item.stack * (int)calcForBuying;
            }
            else
            {
                priceOfStack = itemPrice / 5;
                long num3 = priceOfStack;
                priceOfStack *= item.stack;
                int amount = Main.shopSellbackHelper.GetAmount(item);
                if (amount > 0)
                {
                    priceOfStack += (-num3 + calcForBuying) * Math.Min(amount, item.stack);
                }
            }
            return priceOfStack;
        }

        private static Color RarityColor(Item item) => item != null ? RarityColor(item.rare) : Color.White;
        private static Color RarityColor(int rare)
        {
            if (rare >= ItemRarityID.Count)
                return RarityLoader.GetRarity(rare).RarityColor;

            return ItemRarity.GetColor(rare);
        }

        private static Color TextPulse(Color color) => new(color.R * Main.mouseTextColor / 255, color.G * Main.mouseTextColor / 255, color.B * Main.mouseTextColor / 255, 255);

        private static (int width, int height) MeasureTooltipLines(ReadOnlyCollection<TooltipLine> lines)
        {
            int width = 0;
            int height = -config.spacing;
            foreach (TooltipLine line in lines)
            {
                Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One);
                width = Math.Max(width, (int)size.X + 10);
                height += (int)size.Y + config.spacing;
            }
            return (width, height);
        }

        private static void ClampToScreen(ref int pos, int totalSize, int screenSize, int padding)
        {
            if (pos + totalSize > screenSize) pos = screenSize - totalSize;
            if (pos < padding) pos = padding;
        }

        private static void DrawTooltipBG(int x, int y, int width, int height, Color fillColor, Color borderColor)
        {
            DrawCustomTooltip(Main.spriteBatch, x, y, width, height, Premultiply(fillColor), Premultiply(borderColor));
        }

        private static Color Premultiply(Color c) =>
            new Color(c.R * c.A / 255, c.G * c.A / 255, c.B * c.A / 255, c.A);

        private static void DrawCustomTooltip(SpriteBatch sb, int x, int y, int w, int h, Color fillColor, Color borderColor)
        {
            Texture2D fillTex = ModContent.Request<Texture2D>("TrueTooltips/Assets/InvBgInner").Value;
            Texture2D borderTex = ModContent.Request<Texture2D>("TrueTooltips/Assets/InvBgBorder").Value;
            if (w < 20) w = 20;
            if (h < 20) h = 20;
            DrawNineSlice(sb, fillTex, x, y, w, h, fillColor);
            DrawNineSlice(sb, borderTex, x, y, w, h, borderColor);
        }

        private static void DrawNineSlice(SpriteBatch sb, Texture2D tex, int x, int y, int w, int h, Color c)
        {
            const int corner = 10;
            int texW = tex.Width, texH = tex.Height;

            // corners
            sb.Draw(tex, new Rectangle(x, y, corner, corner), new Rectangle(0, 0, corner, corner), c);
            sb.Draw(tex, new Rectangle(x + w - corner, y, corner, corner), new Rectangle(texW - corner, 0, corner, corner), c);
            sb.Draw(tex, new Rectangle(x, y + h - corner, corner, corner), new Rectangle(0, texH - corner, corner, corner), c);
            sb.Draw(tex, new Rectangle(x + w - corner, y + h - corner, corner, corner), new Rectangle(texW - corner, texH - corner, corner, corner), c);
            // edges
            sb.Draw(tex, new Rectangle(x + corner, y, w - corner * 2, corner), new Rectangle(corner, 0, corner, corner), c);
            sb.Draw(tex, new Rectangle(x + corner, y + h - corner, w - corner * 2, corner), new Rectangle(corner, texH - corner, corner, corner), c);
            sb.Draw(tex, new Rectangle(x, y + corner, corner, h - corner * 2), new Rectangle(0, corner, corner, corner), c);
            sb.Draw(tex, new Rectangle(x + w - corner, y + corner, corner, h - corner * 2), new Rectangle(texW - corner, corner, corner, corner), c);
            // center
            sb.Draw(tex, new Rectangle(x + corner, y + corner, w - corner * 2, h - corner * 2), new Rectangle(corner, corner, corner, corner), c);
        }
    }
}