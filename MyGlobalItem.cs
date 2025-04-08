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

    class MyGlobalItem : GlobalItem
    {

        static readonly Config config = ModContent.GetInstance<Config>();

        static List<int> timerLogs = new List<int>();

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

            var texture = TextureAssets.Item[item.type].Value;
            Rectangle dimensions = Main.itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();

            int x = Main.mouseX + config.x + (Main.ThickMouse ? 6 : 0),
                y = Main.mouseY + config.y + (Main.ThickMouse ? 6 : 0),
                width = 0,
                height = -config.spacing,
                max = new[] { dimensions.Width, dimensions.Height, config.spriteMin }.Max(),
                spriteOffsetX = config.sprite ? max + config.spriteTextPadding : 0,
                borderPadding = config.spriteBorder ? config.spriteBorderPadding : 0,
                index = -1;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (Array.IndexOf(names, lines[i].Name) >= 0)
                {
                    index = i;
                    break;
                }
            }

            foreach (TooltipLine line in lines)
            {
                Vector2 lineSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One);

                width = Math.Max(width, (int)lineSize.X + 10);
                height += (int)lineSize.Y + config.spacing;
                if (config.sprite && lines.IndexOf(line) == index + 1 && height < dimensions.Height)
                    height += dimensions.Height - height;

            }


            if (x + width + config.paddingRight + config.paddingLeft + spriteOffsetX + borderPadding > Main.screenWidth)
            {
                x = _x = Main.screenWidth - width - config.paddingRight - config.paddingLeft - borderPadding - spriteOffsetX;
                _x += 4;
            }

            if (y + height + config.paddingBottom + borderPadding > Main.screenHeight)
            {
                y = _y = Main.screenHeight - height - borderPadding - config.paddingBottom;
                _y += 4;
            }

            if (x - config.paddingLeft < 0)
                x = _x = config.paddingLeft;

            if (y - config.paddingTop < 0)
                y = _y = config.paddingTop;

            _y += config.paddingTop;


            int bgX = x,
                bgY = y,
                bgWidth = width + config.paddingLeft + config.paddingRight,
                bgHeight = height + config.paddingTop + config.paddingBottom;
            if (config.sprite)
            {
                _x += config.spriteTextPadding + spriteOffsetX;
                bgWidth += config.spriteTextPadding + spriteOffsetX;
                int spriteX = x + (max - dimensions.Width) / 2 + borderPadding + config.paddingLeft,
                    spriteY = y + (max - dimensions.Height) / 2 + borderPadding + config.paddingTop;
                if (config.spriteBorder)
                {
                    int borderX = x + config.paddingLeft,
                        borderY = y + config.paddingTop,
                        borderWidth = max + borderPadding * 2,
                        borderHeight = max + borderPadding * 2;
                    bgWidth += borderPadding;
                    _x += borderPadding + 5;
                    Utils.DrawInvBG(Main.spriteBatch, new Rectangle(bgX, bgY, bgWidth, bgHeight), new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
                    Utils.DrawInvBG(Main.spriteBatch, new Rectangle(borderX, borderY, borderWidth, borderHeight), new Color(config.spritebgColor.R * config.spritebgColor.A / 255, config.spritebgColor.G * config.spritebgColor.A / 255, config.spritebgColor.B * config.spritebgColor.A / 255, config.spritebgColor.A));
                }
                else
                {
                    Utils.DrawInvBG(Main.spriteBatch, new Rectangle(bgX, bgY, bgWidth, bgHeight), new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
                }
                Main.spriteBatch.Draw(texture, new Vector2(spriteX, spriteY), dimensions, Color.White);
            }
            else
            {
                Utils.DrawInvBG(Main.spriteBatch, new Rectangle(bgX, bgY, bgWidth, bgHeight), new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
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

            if (config.ammoLine && (item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0))
            {
                foreach (Item invItem in Main.LocalPlayer.inventory)
                {
                    if (invItem.active && (invItem.ammo == item.useAmmo || invItem.bait > 0 || invItem.type == item.tileWand))
                    {
                        currentAmmo = invItem;
                        break;
                    }
                }
            }
            else currentAmmo = null;

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
                    lines.Insert(index, new TooltipLine(Mod, "PriceLine", priceText));
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
                    speed.Text = Math.Round(60 / (item.reuseDelay + (item.useAnimation * (item.CountsAsClass(DamageClass.Melee) ? Main.LocalPlayer.GetAttackSpeed(DamageClass.Melee) : 1))), 2) + Language.GetTextValue("Mods.TrueTooltips.Configs.Config.speedLine.Display");

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

        internal static long GetAdjustedPrice(Item item)
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

        internal static Color RarityColor(Item item) => item != null ? RarityColor(item.rare) : Color.White;
        internal static Color RarityColor(int rare)
        {
            if (rare >= ItemRarityID.Count)
                return RarityLoader.GetRarity(rare).RarityColor;

            return ItemRarity.GetColor(rare);
        }

        internal static Color TextPulse(Color color) => new(color.R * Main.mouseTextColor / 255, color.G * Main.mouseTextColor / 255, color.B * Main.mouseTextColor / 255, 255);
    }
}