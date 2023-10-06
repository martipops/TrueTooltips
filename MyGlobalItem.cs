﻿namespace TrueTooltips
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
    using static Terraria.ID.Colors;
    using static Terraria.Main;
    using static Terraria.ModLoader.ModContent;
    using Terraria.Localization;
    using Humanizer;

    class MyGlobalItem : GlobalItem
    {
        static Color rarityColor;
        static Item currentAmmo;
        static readonly Config config = GetInstance<Config>();

        static readonly string[] names = { "Ammo", "AmmoLine", "AxePower", "BaitPower", "Consumable", "CritChance", "Damage", "Defense", "Equipable", "FishingPower", "HammerPower", "HealLife", "HealMana", "ItemName", "Knockback", "Material", "PickPower", "Placeable", "PriceLine", "Speed", "TileBoost", "UseMana", "Velocity" };

        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int _x, ref int _y)
        {
            var texture = TextureAssets.Item[item.type].Value;
            Rectangle dimensions = itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();

            int x = mouseX + config.x + (ThickMouse ? 6 : 0),
                y = mouseY + config.y + (ThickMouse ? 6 : 0),
                width = 0,
                height = -config.spacing,
                max = new[] { dimensions.Width, dimensions.Height, config.spriteMax }.Max(),
                index = lines.ToList().FindLastIndex(l => names.Contains(l.Name)),
                spriteOffsetX = config.sprite ? max + 10 : 0;


            foreach (TooltipLine line in lines)
            {
                int lineWidth = (int)ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One).X + 10;

                if (lineWidth > width)
                    width = lineWidth + config.paddingRight;

                height += (int)FontAssets.MouseText.Value.MeasureString(line.Text).Y + config.spacing + (lines.IndexOf(line) == index + 1 && config.sprite ? (height < dimensions.Height ? dimensions.Height - height : 0) : 0);
            }


            if (x + width + config.paddingRight + spriteOffsetX > screenWidth)
            {
                x = _x = screenWidth - width - config.paddingRight - spriteOffsetX;
                _x += 4;
            }

            if (y + height + config.paddingBottom > screenHeight)
            {
                y = _y = screenHeight - height - config.paddingBottom;
                _y += 4;
            }

            if (x - config.paddingLeft < 0)
                x = _x = config.paddingLeft;

            if (y - config.paddingTop < 0)
                y = _y = config.paddingTop;

            // adjust the width of the tooltip box with item icon frame
            width += spriteOffsetX + config.paddingRight;

            // adjust the tooltip list x position to make it appear on the right
            _x += spriteOffsetX;

            Utils.DrawInvBG(spriteBatch, new Rectangle(x - config.paddingLeft, y - config.paddingTop, width + config.paddingLeft + config.paddingRight, height + config.paddingTop + config.paddingBottom), new Color(config.bgColor.R * config.bgColor.A / 255, config.bgColor.G * config.bgColor.A / 255, config.bgColor.B * config.bgColor.A / 255, config.bgColor.A));
            if (config.sprite) spriteBatch.Draw(texture, new Vector2(x + (max - dimensions.Width) / 2, y + (max - dimensions.Height) / 2), dimensions, Color.White);

            return true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> lines)
        {
            Config config = GetInstance<Config>();
            float itemKnockback = item.knockBack;
            Player player = LocalPlayer;

            if (item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
            {
                foreach (Item invItem in player.inventory)
                    if (invItem.active && (item.useAmmo > 0 && invItem.ammo == item.useAmmo || item.fishingPole > 0 && invItem.bait > 0 || item.tileWand > 0 && invItem.type == item.tileWand))
                    {
                        currentAmmo = invItem;
                        break;
                    }
                    else currentAmmo = null;

                for (int i = 54; i < 58; i++)
                    if (player.inventory[i].active && item.useAmmo > 0 && player.inventory[i].ammo == item.useAmmo)
                    {
                        currentAmmo = player.inventory[i];
                        break;
                    }
            }
            else currentAmmo = null;

            if (item.type == ItemID.CoinGun)
            {
                int coinGunCrit = (int)(player.GetCritChance(DamageClass.Ranged) - player.inventory[player.selectedItem].crit);

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

            if (config.velocityLine.A > 0 && item.shootSpeed > 0)
                lines.Insert(lines.IndexOf(knockback ?? speed ?? critChance ?? dmg ?? equipable ?? name) + 1, new TooltipLine(Mod, "Velocity", item.shootSpeed + (currentAmmo != null && config.wpnPlusAmmoVelocity ? currentAmmo.shootSpeed : 0) + Language.GetTextValue("Mods.TrueTooltips.Configs.Config.velocityLine.Display")) { OverrideColor = config.velocityLine });

            int index = lines.FindLastIndex(l => names.Contains(l.Name)) + 1;

            if (item.type > ItemID.WormFood && item.type < ItemID.FallenStar)
            {
                if (!lines.Contains(dmg))
                {
                    lines.Insert(1, new TooltipLine(Mod, "Damage", player.GetWeaponDamage(item) + " ranged damage"));

                    dmg = lines.Find(l => l.Name == "Damage");
                }

                lines.Insert(lines.IndexOf(lines.Find(l => l.Name == "Velocity") ?? dmg ?? name) + 1, new TooltipLine(Mod, "Ammo", "Ammo"));
                lines.Add(new TooltipLine(Mod, "Material", "Material"));

                ammo = lines.Find(l => l.Name == "Ammo");
                material = lines.Find(l => l.Name == "Material");
            }

            if (config.priceLine)
            {
                int price = item.stack * (item.buy ? item.GetStoreValue() : item.value / 5);

                int plat = price / 1_000_000,
                    gold = price / 10000 % 100,
                    silver = price / 100 % 100,
                    copper = price % 100;

                if (price > 0 && !(item.type > ItemID.WormFood && item.type < ItemID.FallenStar))
                    lines.Insert(index, new TooltipLine(Mod, "PriceLine", item.buy && item.shopSpecialCurrency >= 0 ? new Regex($@"{Lang.tip[50].Value}\s").Replace(lines.Find(l => l.Name == "SpecialPrice").Text, "", 1) : (plat > 0 ? Language.GetTextValue("Mods.TrueTooltips.Configs.Config.priceLine.platinum").FormatWith(TextPulse(CoinPlatinum).Hex3(), plat) : "") + (gold > 0 ? Language.GetTextValue("Mods.TrueTooltips.Configs.Config.priceLine.gold").FormatWith(TextPulse(CoinGold).Hex3(), gold) : "") + (silver > 0 ? Language.GetTextValue("Mods.TrueTooltips.Configs.Config.priceLine.silver").FormatWith(TextPulse(CoinSilver).Hex3(), silver) : "") + (copper > 0 ? Language.GetTextValue("Mods.TrueTooltips.Configs.Config.priceLine.copper").FormatWith(TextPulse(CoinCopper).Hex3(), copper) : "")));

                lines.RemoveAll(l => l.Name == "Price" || l.Name == "SpecialPrice");
            }

            if (config.ammoLine)
            {
                if (currentAmmo != null)
                    lines.Insert(index, ammoLine);
                else if (item.useAmmo > 0 || item.fishingPole > 0 || item.tileWand > 0)
                    lines.Insert(index, new TooltipLine(Mod, "AmmoLine", Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.No") + (item.fishingPole > 0 ? Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Bait") : new Dictionary<int, string> { [40] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Arrow"), [71] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Coin"), [97] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Bullet"), [169] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Sand"), [283] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Dart"), [771] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Rocket"), [780] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Solution"), [931] = Language.GetTextValue("Mods.TrueTooltips.Configs.Config.ammoLine.Flare") }.TryGetValue(item.useAmmo, out string value) ? value : Lang.GetItemNameValue(item.useAmmo > 0 ? item.useAmmo : item.tileWand))) { OverrideColor = RarityTrash });

                lines.Remove(needsBait);
                lines.Remove(wandConsumes);
            }

            if (config.modName)
            {
                if (currentAmmo?.ModItem != null)
                    ammoLine.Text += " - " + currentAmmo.ModItem.Mod.DisplayName;

                if (item.ModItem != null)
                    name.Text += " - " + item.ModItem.Mod.DisplayName;
            }

            if (ammo != null  && !config.ammo.Equals(Color.White)) ammo.OverrideColor = config.ammo;
            if (axePow != null && !config.axePow.Equals(Color.White)) axePow.OverrideColor = config.axePow;
            if (baitPow != null && !config.baitPow.Equals(Color.White)) baitPow.OverrideColor = config.baitPow;
            if (buffTime != null && !config.buffTime.Equals(Color.White)) buffTime.OverrideColor = config.buffTime;
            if (consumable != null && !config.consumable.Equals(Color.White)) consumable.OverrideColor = config.consumable;
            if (critChance != null && !config.critChance.Equals(Color.White)) critChance.OverrideColor = config.critChance;
            if (defense != null && !config.defense.Equals(Color.White)) defense.OverrideColor = config.defense;

            if (dmg != null)
            {
                if (config.wpnPlusAmmoDmg && currentAmmo != null)
                    dmg.Text = dmg.Text.Replace(dmg.Text.Split(' ').First(), player.GetWeaponDamage(item) + player.GetWeaponDamage(currentAmmo) + "");

                if(!config.dmg.Equals(Color.White)) dmg.OverrideColor = config.dmg;
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

            if (knockback != null)
            {
                if (item.CountsAsClass(DamageClass.Summon))
                    itemKnockback += player.GetKnockback(DamageClass.Summon).Base;

                if (item.type == ItemID.PsychoKnife && player.inventory[player.selectedItem].type == ItemID.PsychoKnife)
                    itemKnockback *= 2 - player.stealth;

                if (item.useAmmo == 1836 || (item.useAmmo == 40 && player.magicQuiver))
                    itemKnockback *= 1.1f;

                itemKnockback = player.GetWeaponKnockback(item);

                if (config.knockbackLine)
                    knockback.Text = Math.Round(itemKnockback + (currentAmmo != null && config.wpnPlusAmmoKb ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0), 2) + Language.GetTextValue("Mods.TrueTooltips.Configs.Config.knockbackLine.Display");

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
                    speed.Text = Math.Round(60 / (item.reuseDelay + (item.useAnimation * (item.CountsAsClass(DamageClass.Melee) ? player.GetAttackSpeed(DamageClass.Melee) : 1))), 2) + Language.GetTextValue("Mods.TrueTooltips.Configs.Config.speedLine.Display");

                if (!config.speed.Equals(Color.White))  speed.OverrideColor = config.speed;
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

            if (config.ammo.A == 0) lines.Remove(ammo);
            if (config.axePow.A == 0) lines.Remove(axePow);
            if (config.badMod.A == 0) lines.RemoveAll(l => l.IsModifierBad);
            if (config.baitPow.A == 0) lines.Remove(baitPow);
            if (config.buffTime.A == 0) lines.Remove(buffTime);
            if (config.consumable.A == 0) lines.Remove(consumable);

            if (config.critChance.A == 0 || (item.ammo > 0 && !config.ammoCrit)) lines.Remove(critChance);
            if (config.defense.A == 0) lines.Remove(defense);
            if (config.dmg.A == 0) lines.Remove(dmg);
            if (config.equipable.A == 0) lines.Remove(equipable);
            if (config.etherianMana.A == 0) lines.Remove(etherianMana);
            if (config.expert.A == 0) lines.Remove(expert);
            if (config.fav.A == 0) lines.Remove(fav);
            if (config.favDescr.A == 0) lines.Remove(favDescr);
            if (config.fishingPow.A == 0) lines.Remove(fishingPow);
            if (config.goodMod.A == 0) lines.RemoveAll(l => !l.IsModifierBad && l.IsModifier);
            if (config.hammerPow.A == 0) lines.Remove(hammerPow);
            if (config.healLife.A == 0) lines.Remove(healLife);
            if (config.healMana.A == 0) lines.Remove(healMana);

            if (config.knockback.A == 0 || itemKnockback + (currentAmmo != null ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0) == 0) lines.Remove(knockback);
            if (config.material.A == 0) lines.Remove(material);
            if (config.needsBait.A == 0) lines.Remove(needsBait);
            if (config.pickPow.A == 0) lines.Remove(pickPow);
            if (config.placeable.A == 0) lines.Remove(placeable);
            if (config.quest.A == 0) lines.Remove(quest);
            if (config.setBonus.A == 0) lines.Remove(setBonus);
            if (config.social.A == 0) lines.Remove(social);
            if (config.socialDescr.A == 0) lines.Remove(socialDescr);

            if (config.speed.A == 0 || (item.type > ItemID.WormFood && item.type < ItemID.FallenStar)) lines.Remove(speed);
            if (config.tileBoost.A == 0) lines.Remove(tileBoost);
            if (config.useMana.A == 0) lines.Remove(useMana);
            if (config.vanity.A == 0) lines.Remove(vanity);
            if (config.wandConsumes.A == 0) lines.Remove(wandConsumes);
            if (config.wellFedExpert.A == 0) lines.Remove(wellFedExpert);
        }

        public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            if (currentAmmo != null)
                rarityColor = RarityColor(currentAmmo);
        }

        internal static Color RarityColor(Item item) => RarityColor(item.rare);
        internal static Color RarityColor(int rare)
        {
            if (rare >= ItemRarityID.Count)
                return RarityLoader.GetRarity(rare).RarityColor;

            return ItemRarity.GetColor(rare);
        }

        internal static Color TextPulse(Color color) => new(color.R * mouseTextColor / 255, color.G * mouseTextColor / 255, color.B * mouseTextColor / 255, mouseTextColor);
    }
}