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
    using Terraria.ID;
    using Terraria.ModLoader;
    using Terraria.UI.Chat;
    using static Terraria.ID.Colors;
    using static Terraria.Main;
    using static Terraria.ModLoader.ModContent;

    class MyGlobalItem : GlobalItem
    {
        static Color rarityColor;
        static Item currentAmmo;

        static readonly string[] names = { "Ammo", "AmmoLine", "AxePower", "BaitPower", "Consumable", "CritChance", "Damage", "Defense", "Equipable", "FishingPower", "HammerPower", "HealLife", "HealMana", "ItemName", "Knockback", "Material", "PickPower", "Placeable", "PriceLine", "Speed", "TileBoost", "UseMana", "Velocity" };

        static readonly Dictionary<int, Color> rarityColors = new()
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
            Rectangle dimensions = item.getRect();

            int x = mouseX + config.x + (ThickMouse ? 6 : 0),
                y = mouseY + config.y + (ThickMouse ? 6 : 0),
                width = 0,
                height = -config.spacing,
                max = new[] { dimensions.Width, dimensions.Height, 50 }.Max(),
                index = lines.ToList().FindLastIndex(l => names.Contains(l.Name));

            foreach(TooltipLine line in lines)
            {
                int lineWidth = (int)ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One).X + (lines.IndexOf(line) <= index && config.sprite ? max + 10 : 0);

                if(lineWidth > width)
                    width = lineWidth;

                height += (int)FontAssets.MouseText.Value.MeasureString(line.Text).Y + config.spacing + (lines.IndexOf(line) == index + 1 && config.sprite ? 10 + (height < dimensions.Height ? dimensions.Height - height : 0) : 0);
            }

            if(x + width + config.paddingRight > screenWidth)
                x = screenWidth - width - config.paddingRight;

            if(y + height + config.paddingBottom > screenHeight)
                y = screenHeight - height - config.paddingBottom;

            if(x - config.paddingLeft < 0)
                x = config.paddingLeft;

            if(y - config.paddingTop < 0)
                y = config.paddingTop;
            Rectangle r = new(x - config.paddingLeft, y - config.paddingTop, width + config.paddingLeft + config.paddingRight, height + config.paddingTop + config.paddingBottom);
            Utils.DrawInvBG(spriteBatch, r, new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
            if(config.sprite) DrawItemIcon(spriteBatch, item, new Vector2(x+max/2, y+max/2), Color.White, max);
            int textureY = y + dimensions.Height;

            foreach(TooltipLine line in lines)
            {
                int yOffset = lines.IndexOf(line) == index + 1 && config.sprite ? 10 + (y < textureY ? textureY - y : 0) : 0;

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, line.Text, new Vector2(x + (lines.IndexOf(line) <= index && config.sprite ? max + 10 : 0), y + yOffset), TextPulse(line.OverrideColor ?? (lines.IndexOf(line) == 0 ? rarityColors[item.rare] : Color.White)), 0, Vector2.Zero, Vector2.One);

                y += (int)FontAssets.MouseText.Value.MeasureString(line.Text).Y + config.spacing + yOffset;
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

            if(item.type == ItemID.CoinGun)
            {
                int coinGunCrit = (int) (player.GetCritChance(DamageClass.Ranged) - player.inventory[player.selectedItem].crit);

                coinGunCrit = player.GetWeaponCrit(item);

                lines.InsertRange(1, new[] { new TooltipLine(Mod, "Damage", "0 ranged damage"), new TooltipLine(Mod, "CritChance", coinGunCrit + "% critical strike chance"), new TooltipLine(Mod, "Speed", ""), new TooltipLine(Mod, "Knockback", "") });
            }

            TooltipLine ammoLine = new(Mod, "AmmoLine", currentAmmo?.HoverName) { OverrideColor = rarityColor },
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
                lines.Insert(lines.IndexOf(knockback ?? speed ?? critChance ?? dmg ?? equipable ?? name) + 1, new TooltipLine(Mod, "Velocity", item.shootSpeed + (currentAmmo != null && config.wpnPlusAmmoVelocity ? currentAmmo.shootSpeed : 0) + " velocity") { OverrideColor = config.velocityLine });

            int index = lines.FindLastIndex(l => names.Contains(l.Name)) + 1;

            if(item.type > 70 && item.type < 75)
            {
                if(!lines.Contains(dmg))
                {
                    lines.Insert(1, new TooltipLine(Mod, "Damage", player.GetWeaponDamage(item) + " ranged damage"));

                    dmg = lines.Find(l => l.Name == "Damage");
                }

                lines.Insert(lines.IndexOf(lines.Find(l => l.Name == "Velocity") ?? dmg ?? name) + 1, new TooltipLine(Mod, "Ammo", "Ammo"));
                lines.Add(new TooltipLine(Mod, "Material", "Material"));

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

                if(price > 0 && !(item.type > ItemID.WormFood && item.type < ItemID.FallenStar))
                    lines.Insert(index, new TooltipLine(Mod, "PriceLine", item.buy && item.shopSpecialCurrency >= 0 ? new Regex($@"{Lang.tip[50].Value}\s").Replace(lines.Find(l => l.Name == "SpecialPrice").Text, "", 1) : (plat > 0 ? $"[c/{TextPulse(CoinPlatinum).Hex3()}:{plat} platinum] " : "") + (gold > 0 ? $"[c/{TextPulse(CoinGold).Hex3()}:{gold} gold] " : "") + (silver > 0 ? $"[c/{TextPulse(CoinSilver).Hex3()}:{silver} silver] " : "") + (copper > 0 ? $"[c/{TextPulse(CoinCopper).Hex3()}:{copper} copper]" : "")));

                lines.RemoveAll(l => l.Name == "Price" || l.Name == "SpecialPrice");
            }

            if(config.ammoLine)
            {
                if(currentAmmo != null)
                    lines.Insert(index, ammoLine);
                else if(item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
                    lines.Insert(index, new TooltipLine(Mod, "AmmoLine", "No " + (item.fishingPole > 0 ? "Bait" : new Dictionary<int, string> { [40] = "Arrow", [71] = "Coin", [97] = "Bullet", [169] = "Sand", [283] = "Dart", [771] = "Rocket", [780] = "Solution", [931] = "Flare" }.TryGetValue(item.useAmmo, out string value) ? value : Lang.GetItemNameValue(item.useAmmo > 0 ? item.useAmmo : item.tileWand))) { OverrideColor = RarityTrash });

                lines.Remove(needsBait);
                lines.Remove(wandConsumes);
            }

            if(config.modName)
            {
                if(currentAmmo?.ModItem != null)
                    ammoLine.Text += " - " + currentAmmo.ModItem.Mod.DisplayName;

                if(item.ModItem != null)
                    name.Text += " - " + item.ModItem.Mod.DisplayName;
            }

            if(ammo != null) ammo.OverrideColor = config.ammo;
            if(axePow != null) axePow.OverrideColor = config.axePow;
            if(baitPow != null) baitPow.OverrideColor = config.baitPow;
            if(buffTime != null) buffTime.OverrideColor = config.buffTime;
            if(consumable != null) consumable.OverrideColor = config.consumable;
            if(critChance != null) critChance.OverrideColor = config.critChance;
            if(defense != null) defense.OverrideColor = config.defense;

            if(dmg != null)
            {
                if(config.wpnPlusAmmoDmg && currentAmmo != null)
                    dmg.Text = dmg.Text.Replace(dmg.Text.Split(' ').First(), player.GetWeaponDamage(item) + player.GetWeaponDamage(currentAmmo) + "");

                dmg.OverrideColor = config.dmg;
            }

            if(equipable != null) equipable.OverrideColor = config.equipable;
            if(etherianMana != null) etherianMana.OverrideColor = config.etherianMana;
            if(expert != null) expert.OverrideColor = config.expert;
            if(fav != null) fav.OverrideColor = config.fav;
            if(favDescr != null) favDescr.OverrideColor = config.favDescr;
            if(fishingPow != null) fishingPow.OverrideColor = config.fishingPow;
            if(hammerPow != null) hammerPow.OverrideColor = config.hammerPow;
            if(healLife != null) healLife.OverrideColor = config.healLife;
            if(healMana != null) healMana.OverrideColor = config.healMana;

            if(knockback != null)
            {
                if(item.CountsAsClass(DamageClass.Summon))
                    itemKnockback += player.GetKnockback(DamageClass.Summon).Base;

                if(item.type == ItemID.PsychoKnife && player.inventory[player.selectedItem].type == 3106)
                    itemKnockback *= 2 - player.stealth;

                if(item.useAmmo == 1836 || (item.useAmmo == 40 && player.magicQuiver))
                    itemKnockback *= 1.1f;

                itemKnockback = player.GetWeaponKnockback(item);

                if(config.knockbackLine)
                    knockback.Text = Math.Round(itemKnockback + (currentAmmo != null && config.wpnPlusAmmoKb ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0), 2) + " knockback";

                knockback.OverrideColor = config.knockback;
            }

            if(material != null) material.OverrideColor = config.material;
            if(needsBait != null) needsBait.OverrideColor = config.needsBait;
            if(pickPow != null) pickPow.OverrideColor = config.pickPow;
            if(placeable != null) placeable.OverrideColor = config.placeable;
            if(quest != null) quest.OverrideColor = config.quest;
            if(setBonus != null) setBonus.OverrideColor = config.setBonus;
            if(social != null) social.OverrideColor = config.social;
            if(socialDescr != null) socialDescr.OverrideColor = config.socialDescr;

            if(speed != null)
            {
                if(config.speedLine)
                    speed.Text = Math.Round(60 / (item.reuseDelay + (item.useAnimation * (item.CountsAsClass(DamageClass.Melee) ? player.GetAttackSpeed(DamageClass.Melee) : 1))), 2) + " attacks per second";

                speed.OverrideColor = config.speed;
            }

            if(tileBoost != null) tileBoost.OverrideColor = config.tileBoost;
            if(useMana != null) useMana.OverrideColor = config.useMana;
            if(vanity != null) vanity.OverrideColor = config.vanity;
            if(wandConsumes != null) wandConsumes.OverrideColor = config.wandConsumes;
            if(wellFedExpert != null) wellFedExpert.OverrideColor = config.wellFedExpert;

            foreach(TooltipLine line in lines)
            {
                if(line.IsModifierBad) line.OverrideColor = config.badMod;
                else if(line.IsModifier) line.OverrideColor = config.goodMod;
            }

            if(config.ammo.A == 0) lines.Remove(ammo);
            if(config.axePow.A == 0) lines.Remove(axePow);
            if(config.badMod.A == 0) lines.RemoveAll(l => l.IsModifierBad);
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
            if(config.goodMod.A == 0) lines.RemoveAll(l => !l.IsModifierBad && l.IsModifier);
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

            return ItemLoader.ModifyTooltips(item, ref var1, new[] { "ItemName" }, ref var2, ref var3, ref var4, ref var5, out _)[0].OverrideColor ?? rarityColors[item.rare];
        }

        internal static Color TextPulse(Color color) => new(color.R * mouseTextColor / 255, color.G * mouseTextColor / 255, color.B * mouseTextColor / 255, mouseTextColor);
    }
}