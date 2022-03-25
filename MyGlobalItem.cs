namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Terraria;
    using Terraria.ModLoader;
    using Terraria.UI.Chat;
    using static Terraria.ID.Colors;
    using static Terraria.Main;
    using static Terraria.ModLoader.ModContent;

    class MyGlobalItem : GlobalItem
    {
        static Color rarityColor;
        static Item currentAmmo;

        static string[] names = { "Ammo", "AmmoLine", "AxePower", "BaitPower", "Consumable", "CritChance", "Damage", "Defense", "Equipable", "FishingPower", "HammerPower", "HealLife", "HealMana", "ItemName", "Knockback", "Material", "PickPower", "Placeable", "PriceLine", "Speed", "TileBoost", "UseMana", "Velocity" };

        static Dictionary<int, Color> rarityColors = new Dictionary<int, Color>
        {
            [-11] = new Color(255, 175, 0),
            [-1] = RarityTrash,
            [0] = RarityNormal,
            [1] = RarityBlue,
            [2] = RarityGreen,
            [3] = RarityOrange,
            [4] = RarityRed,
            [5] = RarityPink,
            [6] = RarityPurple,
            [7] = RarityLime,
            [8] = RarityYellow,
            [9] = RarityCyan,
            [10] = new Color(255, 40, 100),
            [11] = new Color(180, 40, 255)
        };

        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int _x, ref int _y)
        {
            Config config = GetInstance<Config>();
            var texture = itemTexture[item.type];
            Rectangle dimensions = itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();

            int x = mouseX + config.x + (ThickMouse ? 6 : 0),
                y = mouseY + config.y + (ThickMouse ? 6 : 0),
                width = 0,
                height = -config.spacing,
                max = new[] { dimensions.Width, dimensions.Height, 50 }.Max(),
                index = lines.ToList().FindLastIndex(l => names.Contains(l.Name));

            foreach(TooltipLine line in lines)
            {
                int lineWidth = (int)ChatManager.GetStringSize(fontMouseText, line.text, Vector2.One).X + (lines.IndexOf(line) <= index && config.sprite ? max + 10 : 0);

                if(lineWidth > width)
                    width = lineWidth;

                height += (int)fontMouseText.MeasureString(line.text).Y + config.spacing + (lines.IndexOf(line) == index + 1 && config.sprite ? 10 + (height < dimensions.Height ? dimensions.Height - height : 0) : 0);
            }

            if(x + width + config.paddingRight > screenWidth)
                x = screenWidth - width - config.paddingRight;

            if(y + height + config.paddingBottom > screenHeight)
                y = screenHeight - height - config.paddingBottom;

            if(x - config.paddingLeft < 0)
                x = config.paddingLeft;

            if(y - config.paddingTop < 0)
                y = config.paddingTop;

            Utils.DrawInvBG(spriteBatch, new Rectangle(x - config.paddingLeft, y - config.paddingTop, width + config.paddingLeft + config.paddingRight, height + config.paddingTop + config.paddingBottom), new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
            if(config.sprite) spriteBatch.Draw(texture, new Vector2(x + (max - dimensions.Width) / 2, y), dimensions, Color.White);

            int textureY = y + dimensions.Height;

            foreach(TooltipLine line in lines)
            {
                int yOffset = lines.IndexOf(line) == index + 1 && config.sprite ? 10 + (y < textureY ? textureY - y : 0) : 0;

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, fontMouseText, line.text, new Vector2(x + (lines.IndexOf(line) <= index && config.sprite ? max + 10 : 0), y + yOffset), TextPulse(line.overrideColor ?? (lines.IndexOf(line) == 0 ? rarityColors[item.rare] : Color.White)), 0, Vector2.Zero, Vector2.One);

                y += (int)fontMouseText.MeasureString(line.text).Y + config.spacing + yOffset;
            }

            return false;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> lines)
        {
            Config config = GetInstance<Config>();
            float itemKnockback = item.knockBack;
            Player player = LocalPlayer;

            if(item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
            {
                foreach(Item invItem in player.inventory)
                    if(invItem.active && (item.useAmmo > 0 && invItem.ammo == item.useAmmo || item.fishingPole > 0 && invItem.bait > 0 || item.tileWand > 0 && invItem.type == item.tileWand))
                    {
                        currentAmmo = invItem;
                        break;
                    }
                    else currentAmmo = null;

                for(int i = 54; i < 58; i++)
                    if(player.inventory[i].active && item.useAmmo > 0 && player.inventory[i].ammo == item.useAmmo)
                    {
                        currentAmmo = player.inventory[i];
                        break;
                    }
            }
            else currentAmmo = null;

            if(item.type == 905)
            {
                int coinGunCrit = player.rangedCrit - player.inventory[player.selectedItem].crit;

                ItemLoader.GetWeaponCrit(item, player, ref coinGunCrit);
                PlayerHooks.GetWeaponCrit(player, item, ref coinGunCrit);

                lines.InsertRange(1, new[] { new TooltipLine(mod, "Damage", "0 ranged damage"), new TooltipLine(mod, "CritChance", coinGunCrit + "% critical strike chance"), new TooltipLine(mod, "Speed", ""), new TooltipLine(mod, "Knockback", "") });
            }

            TooltipLine ammoLine = new TooltipLine(mod, "AmmoLine", currentAmmo?.HoverName) { overrideColor = rarityColor },
                        ammo = lines.Find(l => l.Name == "Ammo"),
                        axePow = lines.Find(l => l.Name == "AxePower"),
                        baitPow = lines.Find(l => l.Name == "BaitPower"),
                        buffTime = lines.Find(l => l.Name == "BuffTime"),
                        consumable = lines.Find(l => l.Name == "Consumable"),
                        critChance = lines.Find(l => l.Name == "CritChance"),
                        defense = lines.Find(l => l.Name == "Defense"),
                        dmg = lines.Find(l => l.Name == "Damage"),
                        equipable = lines.Find(l => l.Name == "Equipable"),
                        etherianMana = lines.Find(l => l.Name == "EtherianManaWarning"),
                        expert = lines.Find(l => l.Name == "Expert"),
                        fav = lines.Find(l => l.Name == "Favorite"),
                        favDescr = lines.Find(l => l.Name == "FavoriteDesc"),
                        fishingPow = lines.Find(l => l.Name == "FishingPower"),
                        hammerPow = lines.Find(l => l.Name == "HammerPower"),
                        healLife = lines.Find(l => l.Name == "HealLife"),
                        healMana = lines.Find(l => l.Name == "HealMana"),
                        knockback = lines.Find(l => l.Name == "Knockback"),
                        material = lines.Find(l => l.Name == "Material"),
                        name = lines.Find(l => l.Name == "ItemName"),
                        needsBait = lines.Find(l => l.Name == "NeedsBait"),
                        pickPow = lines.Find(l => l.Name == "PickPower"),
                        placeable = lines.Find(l => l.Name == "Placeable"),
                        quest = lines.Find(l => l.Name == "Quest"),
                        setBonus = lines.Find(l => l.Name == "SetBonus"),
                        social = lines.Find(l => l.Name == "Social"),
                        socialDescr = lines.Find(l => l.Name == "SocialDesc"),
                        speed = lines.Find(l => l.Name == "Speed"),
                        tileBoost = lines.Find(l => l.Name == "TileBoost"),
                        useMana = lines.Find(l => l.Name == "UseMana"),
                        vanity = lines.Find(l => l.Name == "Vanity"),
                        wandConsumes = lines.Find(l => l.Name == "WandConsumes"),
                        wellFedExpert = lines.Find(l => l.Name == "WellFedExpert");

            if(config.velocityLine.A > 0 && item.shootSpeed > 0)
                lines.Insert(lines.IndexOf(knockback ?? speed ?? critChance ?? dmg ?? equipable ?? name) + 1, new TooltipLine(mod, "Velocity", item.shootSpeed + (currentAmmo != null && config.wpnPlusAmmoVelocity ? currentAmmo.shootSpeed : 0) + " velocity") { overrideColor = config.velocityLine });

            int index = lines.FindLastIndex(l => names.Contains(l.Name)) + 1;

            if(item.type > 70 && item.type < 75)
            {
                if(!lines.Contains(dmg))
                {
                    lines.Insert(1, new TooltipLine(mod, "Damage", player.GetWeaponDamage(item) + " ranged damage"));

                    dmg = lines.Find(l => l.Name == "Damage");
                }

                lines.Insert(lines.IndexOf(lines.Find(l => l.Name == "Velocity") ?? dmg ?? name) + 1, new TooltipLine(mod, "Ammo", "Ammo"));
                lines.Add(new TooltipLine(mod, "Material", "Material"));

                ammo = lines.Find(l => l.Name == "Ammo");
                material = lines.Find(l => l.Name == "Material");
            }

            if(config.priceLine)
            {
                int price = item.stack * (item.buy ? item.GetStoreValue() : item.value / 5);

                int plat = price / 1_000_000,
                    gold = price / 10000 % 100,
                    silver = price / 100 % 100,
                    copper = price % 100;

                if(price > 0 && !(item.type > 70 && item.type < 75))
                    lines.Insert(index, new TooltipLine(mod, "PriceLine", item.buy && item.shopSpecialCurrency >= 0 ? new Regex($@"{Lang.tip[50].Value}\s").Replace(lines.Find(l => l.Name == "SpecialPrice").text, "", 1) : (plat > 0 ? $"[c/{TextPulse(CoinPlatinum).Hex3()}:{plat} platinum] " : "") + (gold > 0 ? $"[c/{TextPulse(CoinGold).Hex3()}:{gold} gold] " : "") + (silver > 0 ? $"[c/{TextPulse(CoinSilver).Hex3()}:{silver} silver] " : "") + (copper > 0 ? $"[c/{TextPulse(CoinCopper).Hex3()}:{copper} copper]" : "")));

                lines.RemoveAll(l => l.Name == "Price" || l.Name == "SpecialPrice");
            }

            if(config.ammoLine)
            {
                if(currentAmmo != null)
                    lines.Insert(index, ammoLine);
                else if(item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
                    lines.Insert(index, new TooltipLine(mod, "AmmoLine", "No " + (item.fishingPole > 0 ? "Bait" : new Dictionary<int, string> { [40] = "Arrow", [71] = "Coin", [97] = "Bullet", [169] = "Sand", [283] = "Dart", [771] = "Rocket", [780] = "Solution", [931] = "Flare" }.TryGetValue(item.useAmmo, out string value) ? value : Lang.GetItemNameValue(item.useAmmo > 0 ? item.useAmmo : item.tileWand))) { overrideColor = RarityTrash });

                lines.Remove(needsBait);
                lines.Remove(wandConsumes);
            }

            if(config.modName)
            {
                if(currentAmmo?.modItem != null)
                    ammoLine.text += " - " + currentAmmo.modItem.mod.DisplayName;

                if(item.modItem != null)
                    name.text += " - " + item.modItem.mod.DisplayName;
            }

            if(ammo != null) ammo.overrideColor = config.ammo;
            if(axePow != null) axePow.overrideColor = config.axePow;
            if(baitPow != null) baitPow.overrideColor = config.baitPow;
            if(buffTime != null) buffTime.overrideColor = config.buffTime;
            if(consumable != null) consumable.overrideColor = config.consumable;
            if(critChance != null) critChance.overrideColor = config.critChance;
            if(defense != null) defense.overrideColor = config.defense;

            if(dmg != null)
            {
                if(config.wpnPlusAmmoDmg && currentAmmo != null)
                    dmg.text = dmg.text.Replace(dmg.text.Split(' ').First(), player.GetWeaponDamage(item) + player.GetWeaponDamage(currentAmmo) + "");

                if(ModLoader.GetMod("_ColoredDamageTypes") == null)
                    dmg.overrideColor = config.dmg;
            }

            if(equipable != null) equipable.overrideColor = config.equipable;
            if(etherianMana != null) etherianMana.overrideColor = config.etherianMana;
            if(expert != null) expert.overrideColor = config.expert;
            if(fav != null) fav.overrideColor = config.fav;
            if(favDescr != null) favDescr.overrideColor = config.favDescr;
            if(fishingPow != null) fishingPow.overrideColor = config.fishingPow;
            if(hammerPow != null) hammerPow.overrideColor = config.hammerPow;
            if(healLife != null) healLife.overrideColor = config.healLife;
            if(healMana != null) healMana.overrideColor = config.healMana;

            if(knockback != null)
            {
                if(item.summon)
                    itemKnockback += player.minionKB;

                if(item.type == 3106 && player.inventory[player.selectedItem].type == 3106)
                    itemKnockback *= 2 - player.stealth;

                if(item.useAmmo == 1836 || (item.useAmmo == 40 && player.magicQuiver))
                    itemKnockback *= 1.1f;

                ItemLoader.GetWeaponKnockback(item, player, ref itemKnockback);
                PlayerHooks.GetWeaponKnockback(player, item, ref itemKnockback);

                if(config.knockbackLine)
                    knockback.text = Math.Round(itemKnockback + (currentAmmo != null && config.wpnPlusAmmoKb ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0), 2) + " knockback";

                knockback.overrideColor = config.knockback;
            }

            if(material != null) material.overrideColor = config.material;
            if(needsBait != null) needsBait.overrideColor = config.needsBait;
            if(pickPow != null) pickPow.overrideColor = config.pickPow;
            if(placeable != null) placeable.overrideColor = config.placeable;
            if(quest != null) quest.overrideColor = config.quest;
            if(setBonus != null) setBonus.overrideColor = config.setBonus;
            if(social != null) social.overrideColor = config.social;
            if(socialDescr != null) socialDescr.overrideColor = config.socialDescr;

            if(speed != null)
            {
                if(config.speedLine)
                    speed.text = Math.Round(60 / (item.reuseDelay + (item.useAnimation * (item.melee ? player.meleeSpeed : 1))), 2) + " attacks per second";

                speed.overrideColor = config.speed;
            }

            if(tileBoost != null) tileBoost.overrideColor = config.tileBoost;
            if(useMana != null) useMana.overrideColor = config.useMana;
            if(vanity != null) vanity.overrideColor = config.vanity;
            if(wandConsumes != null) wandConsumes.overrideColor = config.wandConsumes;
            if(wellFedExpert != null) wellFedExpert.overrideColor = config.wellFedExpert;

            foreach(TooltipLine line in lines)
            {
                if(line.isModifierBad) line.overrideColor = config.badMod;
                else if(line.isModifier) line.overrideColor = config.goodMod;
            }

            if(config.ammo.A == 0) lines.Remove(ammo);
            if(config.axePow.A == 0) lines.Remove(axePow);
            if(config.badMod.A == 0) lines.RemoveAll(l => l.isModifierBad);
            if(config.baitPow.A == 0) lines.Remove(baitPow);
            if(config.buffTime.A == 0) lines.Remove(buffTime);
            if(config.consumable.A == 0) lines.Remove(consumable);

            if(config.critChance.A == 0 || (item.ammo > 0 && !config.ammoCrit)) lines.Remove(critChance);
            if(config.defense.A == 0) lines.Remove(defense);
            if(config.dmg.A == 0) lines.Remove(dmg);
            if(config.equipable.A == 0) lines.Remove(equipable);
            if(config.etherianMana.A == 0) lines.Remove(etherianMana);
            if(config.expert.A == 0) lines.Remove(expert);
            if(config.fav.A == 0) lines.Remove(fav);
            if(config.favDescr.A == 0) lines.Remove(favDescr);
            if(config.fishingPow.A == 0) lines.Remove(fishingPow);
            if(config.goodMod.A == 0) lines.RemoveAll(l => !l.isModifierBad && l.isModifier);
            if(config.hammerPow.A == 0) lines.Remove(hammerPow);
            if(config.healLife.A == 0) lines.Remove(healLife);
            if(config.healMana.A == 0) lines.Remove(healMana);

            if(config.knockback.A == 0 || itemKnockback + (currentAmmo != null ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0) == 0) lines.Remove(knockback);
            if(config.material.A == 0) lines.Remove(material);
            if(config.needsBait.A == 0) lines.Remove(needsBait);
            if(config.pickPow.A == 0) lines.Remove(pickPow);
            if(config.placeable.A == 0) lines.Remove(placeable);
            if(config.quest.A == 0) lines.Remove(quest);
            if(config.setBonus.A == 0) lines.Remove(setBonus);
            if(config.social.A == 0) lines.Remove(social);
            if(config.socialDescr.A == 0) lines.Remove(socialDescr);

            if(config.speed.A == 0 || (item.type > 70 && item.type < 75)) lines.Remove(speed);
            if(config.tileBoost.A == 0) lines.Remove(tileBoost);
            if(config.useMana.A == 0) lines.Remove(useMana);
            if(config.vanity.A == 0) lines.Remove(vanity);
            if(config.wandConsumes.A == 0) lines.Remove(wandConsumes);
            if(config.wellFedExpert.A == 0) lines.Remove(wellFedExpert);
        }

        public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            if(currentAmmo != null)
                rarityColor = RarityColor(currentAmmo);
        }

        internal static Color RarityColor(Item item)
        {
            int var1 = 1;
            string[] var2 = { "" };
            var var3 = new bool[1];
            var var4 = new bool[1];
            int var5 = -1;

            return ItemLoader.ModifyTooltips(item, ref var1, new[] { "ItemName" }, ref var2, ref var3, ref var4, ref var5, out _)[0].overrideColor ?? rarityColors[item.rare];
        }

        internal static Color TextPulse(Color color) => new Color(color.R * mouseTextColor / 255, color.G * mouseTextColor / 255, color.B * mouseTextColor / 255, mouseTextColor);
    }
}