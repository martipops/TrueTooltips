namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Terraria;
    using Terraria.ModLoader;
    using static Terraria.ID.Colors;
    using static Terraria.Main;
    using static Terraria.ModLoader.ModContent;

    class MyGlobalItem : GlobalItem
    {
        static Color rarityColor;
        static Item currentAmmo;

        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            Color bgColor = GetInstance<Config>().bgColor;

            // Get the texts of all tooltip lines for calculating width and height of the background but remove the parts of the priceLine line's text that only change the color.
            IEnumerable<string> lineTexts = lines.Select(l => l.mod == mod.Name && l.Name == "priceLine" ? System.Text.RegularExpressions.Regex.Replace(l.text, @"\[.{9}|]|\s$", "") : l.text);

            // Draw the background; As wide as the widest line and as high as the sum of the height of all lines, + padding.
            if(bgColor.A > 0)
                Utils.DrawInvBG(spriteBatch, new Rectangle(x - 10, y - 10, (int)lineTexts.Max(l => fontMouseText.MeasureString(l).X) + 20, (int)lineTexts.Sum(l => fontMouseText.MeasureString(l).Y) + 15), new Color(bgColor.R * bgColor.A / 255, bgColor.G * bgColor.A / 255, bgColor.B * bgColor.A / 255, bgColor.A));

            return base.PreDrawTooltip(item, lines, ref x, ref y);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> lines)
        {
            Config config = GetInstance<Config>();
            float itemKnockback = item.knockBack;
            Player player = LocalPlayer;

            // If the item can use ammo, search through the player's inventory, then the ammo slots.
            // If the inventory-item is the matching ammo, assign it to currentAmmo.
            if(item.useAmmo > 0)
            {
                foreach(Item invItem in player.inventory)
                    if(invItem.active && invItem.ammo == item.useAmmo)
                    {
                        currentAmmo = invItem;
                        break;
                    }
                    else currentAmmo = null;

                for(int i = 54; i < 58; i++)
                    if(player.inventory[i].active && player.inventory[i].ammo == item.useAmmo)
                    {
                        currentAmmo = player.inventory[i];
                        break;
                    }
            }
            else currentAmmo = null;

            TooltipLine ammoLine = new TooltipLine(mod, "", currentAmmo?.HoverName) { overrideColor = rarityColor },
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

            // Calculate the correct knockback value. {
            if(item.summon)
                itemKnockback += player.minionKB;

            if(item.type == 3106 && player.inventory[player.selectedItem].type == 3106)
                itemKnockback *= 2 - player.stealth;

            if(item.useAmmo == 1836 || (item.useAmmo == 40 && player.magicQuiver))
                itemKnockback *= 1.1f;

            ItemLoader.GetWeaponKnockback(item, player, ref itemKnockback);
            PlayerHooks.GetWeaponKnockback(player, item, ref itemKnockback);
            // }

            // Add the ammo line if the item has ammo, otherwise add a line showing what ammo the item needs.
            if(config.ammoLine && item.useAmmo > 0)
            {
                if(currentAmmo != null)
                    lines.Add(ammoLine);
                else lines.Add(new TooltipLine(mod, "", "No " + (
                    new Dictionary<int, string>
                    {
                        [40] = "Arrow",
                        [71] = "Coin",
                        [97] = "Bullet",
                        [169] = "Sand",
                        [283] = "Dart",
                        [771] = "Rocket",
                        [780] = "Solution",
                        [931] = "Flare"
                    }.TryGetValue(item.useAmmo, out string value) ? value : Lang.GetItemNameValue(item.useAmmo)))
                { overrideColor = RarityTrash });
            }

            // Add the price line if the item isn't a coin.
            if(config.priceLine)
            {
                int price = item.stack * (item.buy ? item.GetStoreValue() : item.value / 5);

                int plat = price / 1_000_000,
                    gold = price / 10000 % 100,
                    silver = price / 100 % 100,
                    copper = price % 100;

                if(!(item.type > 70 && item.type < 75))
                    lines.Add(new TooltipLine(mod, "priceLine", item.buy && item.shopSpecialCurrency == 0 ? $"[c/{TextPulse(new Color(240, 100, 120)).Hex3()}:{price} defender medals]" : (plat > 0 ? $"[c/{TextPulse(CoinPlatinum).Hex3()}:{plat} platinum] " : "") + (gold > 0 ? $"[c/{TextPulse(CoinGold).Hex3()}:{gold} gold] " : "") + (silver > 0 ? $"[c/{TextPulse(CoinSilver).Hex3()}:{silver} silver] " : "") + (copper > 0 ? $"[c/{TextPulse(CoinCopper).Hex3()}:{copper} copper]" : "")));

                lines.RemoveAll(l => l.Name == "Price" || l.Name == "SpecialPrice");
            }

            // Show the mod that adds the item.
            if(config.modName)
            {
                if(currentAmmo?.modItem != null)
                    ammoLine.text += " - " + currentAmmo.modItem.mod.DisplayName;

                if(item.modItem != null)
                    lines.Find(l => l.Name == "ItemName").text += " - " + item.modItem.mod.DisplayName;
            }

            if(ammo != null) ammo.overrideColor = config.ammo;
            if(axePow != null) axePow.overrideColor = config.axePow;
            if(baitPow != null) baitPow.overrideColor = config.baitPow;
            if(buffTime != null) buffTime.overrideColor = config.buffTime;
            if(consumable != null) consumable.overrideColor = config.consumable;
            if(critChance != null) critChance.overrideColor = config.critChance;
            if(defense != null) defense.overrideColor = config.defense;

            // Replace damage value with sum of weapon- and ammo damage.
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

            // Show knockback as number and combine weapon- and ammo knockback.
            if(knockback != null)
            {
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

            // Show attack speed as number.
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

            // Recolor modifier lines.
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

            // Remove crit line from ammo.
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

            // Remove knockback line if item has no knockback.
            if(config.knockback.A == 0 || itemKnockback + (currentAmmo != null ? player.GetWeaponKnockback(currentAmmo, currentAmmo.knockBack) : 0) == 0) lines.Remove(knockback);
            if(config.material.A == 0) lines.Remove(material);
            if(config.needsBait.A == 0) lines.Remove(needsBait);
            if(config.pickPow.A == 0) lines.Remove(pickPow);
            if(config.placeable.A == 0) lines.Remove(placeable);
            if(config.quest.A == 0) lines.Remove(quest);
            if(config.setBonus.A == 0) lines.Remove(setBonus);
            if(config.social.A == 0) lines.Remove(social);
            if(config.socialDescr.A == 0) lines.Remove(socialDescr);
            if(config.speed.A == 0) lines.Remove(speed);
            if(config.tileBoost.A == 0) lines.Remove(tileBoost);
            if(config.useMana.A == 0) lines.Remove(useMana);
            if(config.vanity.A == 0) lines.Remove(vanity);
            if(config.wandConsumes.A == 0) lines.Remove(wandConsumes);
            if(config.wellFedExpert.A == 0) lines.Remove(wellFedExpert);
        }

        public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            // Calling RarityColor in ModifyTooltips causes crash because of an infinite loop.
            if(currentAmmo != null)
                rarityColor = RarityColor(currentAmmo);
        }

        /// <summary>
        /// Returns the rarity color, including custom rarity color, of the input item.
        /// </summary>
        // Calls all methods overriding ModifyTooltips with the input item and a list containing a TooltipLine object with "ItemName" assigned to its Name field as arguments.
        // The overriding methods then assign to the TooltipLine object's overrideColor field and the list is returned.
        internal static Color RarityColor(Item item)
        {
            int var1 = 1;
            string[] var2 = { "" };
            var var3 = new bool[1];
            var var4 = new bool[1];
            int var5 = -1;

            return ItemLoader.ModifyTooltips(item, ref var1, new[] { "ItemName" }, ref var2, ref var3, ref var4, ref var5, out _)[0].overrideColor ??
                new Dictionary<int, Color>
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
                }[item.rare];
        }

        /// <summary>
        /// Makes the input color pulse and returns it.
        /// </summary>
        internal static Color TextPulse(Color color) => new Color(color.R * mouseTextColor / 255, color.G * mouseTextColor / 255, color.B * mouseTextColor / 255, mouseTextColor);
    }
}