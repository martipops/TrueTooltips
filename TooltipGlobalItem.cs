using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace TrueTooltips;

internal class TooltipGlobalItem : GlobalItem
{
    private const string LocalizationRoot = "Mods.TrueTooltips.Configs.Config.";

    // Extra width per line, kept so tooltip widths stay as they have always been.
    private const int TextWidthPadding = 10;

    // Size in pixels of the corners in the panel textures.
    private const int PanelCornerSize = 10;

    // The ammo line goes right after the last of these, ahead of the item's description.
    private static readonly HashSet<string> LinesBeforeAmmo = new()
    {
        "Ammo", "AxePower", "BaitPower", "Consumable", "CritChance", "Damage", "Defense", "Equipable", "FishingPower",
        "HammerPower", "HealLife", "HealMana", "ItemName", "Knockback", "Material", "PickPower", "Placeable", "Speed",
        "TileBoost", "UseMana", "Velocity", "WingTime",
    };

    // Ammo types whose name isn't simply an item's name. The rest are named after the item with that ID.
    private static readonly Dictionary<int, string> AmmoTypeNames = new()
    {
        [AmmoID.Arrow] = "Arrow",
        [AmmoID.Coin] = "Coin",
        [AmmoID.Bullet] = "Bullet",
        [AmmoID.Sand] = "Sand",
        [AmmoID.Dart] = "Dart",
        [AmmoID.Rocket] = "Rocket",
        [AmmoID.Solution] = "Solution",
        [AmmoID.Flare] = "Flare",
    };

    private static Config Config => ModContent.GetInstance<Config>();

    private static Player Player => Main.LocalPlayer;

    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        Config config = Config;

        // Item textures load on demand, so this one might not be available yet.
        Main.instance.LoadItem(item.type);
        Texture2D texture = TextureAssets.Item[item.type].Value;
        Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();

        int textWidth = 0;
        int textHeight = -config.spacing;
        foreach (TooltipLine line in lines)
        {
            Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One);
            textWidth = Math.Max(textWidth, (int)size.X + TextWidthPadding);
            textHeight += (int)size.Y + config.spacing;
        }

        // The icon box is a square, large enough for the sprite plus its border padding.
        int iconSize = Math.Max(Math.Max(frame.Width, frame.Height), config.spriteMin);
        int borderPadding = config.spriteBorder ? config.spriteBorderPadding : 0;
        int iconBoxSize = config.sprite ? iconSize + borderPadding * 2 : 0;
        int textOffsetX = config.sprite ? iconBoxSize + config.spriteTextPadding : 0;

        int panelWidth = config.paddingLeft + textOffsetX + textWidth + config.paddingRight;
        int panelHeight = config.paddingTop + Math.Max(textHeight, iconBoxSize) + config.paddingBottom;

        // If the panel is bigger than the screen, keep its top left corner visible.
        int panelX = Math.Max(0, Math.Min(x + config.x, Main.screenWidth - panelWidth));
        int panelY = Math.Max(0, Math.Min(y + config.y, Main.screenHeight - panelHeight));

        DrawPanel(new Rectangle(panelX, panelY, panelWidth, panelHeight), config.bgColor, config.borderColor);

        if (config.sprite)
        {
            int boxX = panelX + config.paddingLeft;
            int boxY = panelY + config.paddingTop;

            if (config.spriteBorder)
                DrawPanel(new Rectangle(boxX, boxY, iconBoxSize, iconBoxSize), config.spritebgColor, config.borderColor);

            var spritePosition = new Vector2(
                boxX + borderPadding + (iconSize - frame.Width) / 2,
                boxY + borderPadding + (iconSize - frame.Height) / 2);

            // Same as how the game draws items in inventory slots: tinted items like Gel need a second pass.
            Main.spriteBatch.Draw(texture, spritePosition, frame, item.GetAlpha(Color.White));
            if (item.color != default)
                Main.spriteBatch.Draw(texture, spritePosition, frame, item.GetColor(Color.White));
        }

        x = panelX + config.paddingLeft + textOffsetX;
        y = panelY + config.paddingTop;
        return true;
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (Config.spacing != 0)
            yOffset = Config.spacing;

        return true;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> lines)
    {
        Config config = Config;
        Item ammo = config.ammoLine ? FindAmmo(item) : null;

        if (item.type == ItemID.CoinGun)
            AddCoinGunLines(item, lines, config);

        if (config.velocityLine.A > 0 && item.shootSpeed > 0)
            AddVelocityLine(item, ammo, lines, config);

        if (item.IsACoin)
            AddCoinLines(item, lines);

        if (config.wingFlyTimeLine && item.wingSlot > 0)
            AddWingTimeLine(item, lines);

        TooltipLine ammoLine = config.ammoLine ? AddAmmoLine(item, ammo, lines) : null;

        if (config.priceLine)
            ReplacePriceLine(item, lines);

        AddModName(item, ammo, ammoLine, lines, config);
        ImproveDamageLine(item, ammo, lines, config);

        if (config.knockbackLine)
            ImproveKnockbackLine(item, ammo, lines, config);

        if (config.speedLine)
            ImproveSpeedLine(item, lines);

        if (item.ammo > 0 && !config.ammoCrit)
            lines.Find(l => l.Name == "CritChance")?.Hide();

        ApplyLineColors(lines, config);
    }

    private static void DrawPanel(Rectangle area, Color fill, Color border)
    {
        area.Width = Math.Max(area.Width, PanelCornerSize * 2);
        area.Height = Math.Max(area.Height, PanelCornerSize * 2);

        DrawNineSlice(ModContent.Request<Texture2D>("TrueTooltips/Assets/InvBgInner").Value, area, fill);
        DrawNineSlice(ModContent.Request<Texture2D>("TrueTooltips/Assets/InvBgBorder").Value, area, border);
    }

    /// <summary>Stretches a texture over an area while keeping its corners sharp.</summary>
    private static void DrawNineSlice(Texture2D texture, Rectangle area, Color color)
    {
        const int corner = PanelCornerSize;
        color = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);

        int[] screenX = { area.Left, area.Left + corner, area.Right - corner };
        int[] screenY = { area.Top, area.Top + corner, area.Bottom - corner };
        int[] sourceX = { 0, corner, texture.Width - corner };
        int[] sourceY = { 0, corner, texture.Height - corner };
        int[] width = { corner, area.Width - corner * 2, corner };
        int[] height = { corner, area.Height - corner * 2, corner };

        for (int column = 0; column < 3; column++)
        {
            for (int row = 0; row < 3; row++)
            {
                var destination = new Rectangle(screenX[column], screenY[row], width[column], height[row]);
                var source = new Rectangle(sourceX[column], sourceY[row], corner, corner);
                Main.spriteBatch.Draw(texture, destination, source, color);
            }
        }
    }

    /// <summary>What the item would consume right now, in the same order the game picks it. Null if it has none.</summary>
    private static Item FindAmmo(Item item)
    {
        if (item.fishingPole > 0)
        {
            // Player.Fishing_GetBait is private: ammo slots first, then the main inventory.
            Item[] inventory = Player.inventory;
            return inventory[54..58].Concat(inventory[..50]).FirstOrDefault(i => i.stack > 0 && i.bait > 0);
        }

        if (item.useAmmo > 0)
            return Player.ChooseAmmo(item);

        if (item.tileWand > 0)
            return Array.Find(Player.inventory, i => i.type == item.tileWand && i.stack > 0);

        return null;
    }

    /// <summary>The Coin Gun has no damage of its own, so the game leaves out its stat lines.</summary>
    private void AddCoinGunLines(Item item, List<TooltipLine> lines, Config config)
    {
        var stats = new List<TooltipLine>
        {
            new(Mod, "Damage", "0" + Language.GetTextValue("LegacyTooltip.3")),
            new(Mod, "CritChance", Player.GetWeaponCrit(item) + Language.GetTextValue("LegacyTooltip.41")),
        };

        // These start empty and get their text from ImproveSpeedLine and ImproveKnockbackLine.
        if (config.speedLine)
            stats.Add(new(Mod, "Speed", ""));
        if (config.knockbackLine)
            stats.Add(new(Mod, "Knockback", ""));

        lines.InsertRange(1, stats);
    }

    private void AddVelocityLine(Item item, Item ammo, List<TooltipLine> lines, Config config)
    {
        float velocity = item.shootSpeed + (ammo != null && config.wpnPlusAmmoVelocity ? ammo.shootSpeed : 0);
        InsertAfterFirstFound(
            lines,
            // LegacyTooltip.44 is "% velocity", the percent sign is dropped.
            new TooltipLine(Mod, "Velocity", Math.Round(velocity, 2) + Language.GetTextValue("LegacyTooltip.44")[1..]),
            "Knockback", "Speed", "CritChance", "Damage", "Equipable", "ItemName");
    }

    /// <summary>Coins are ammo for the Coin Gun, but the game doesn't show them as such.</summary>
    private void AddCoinLines(Item item, List<TooltipLine> lines)
    {
        if (!lines.Exists(l => l.Name == "Damage"))
            lines.Insert(1, new TooltipLine(Mod, "Damage", Player.GetWeaponDamage(item) + Language.GetTextValue("LegacyTooltip.3")));

        InsertAfterFirstFound(
            lines,
            new TooltipLine(Mod, "Ammo", Language.GetTextValue("LegacyTooltip.34")),
            "Velocity", "Damage", "ItemName");

        lines.Add(new TooltipLine(Mod, "Material", Language.GetTextValue("LegacyTooltip.36")));
    }

    private void AddWingTimeLine(Item item, List<TooltipLine> lines)
    {
        int flyTime = ArmorIDs.Wing.Sets.Stats[item.wingSlot].FlyTime;
        InsertAfterFirstFound(
            lines,
            new TooltipLine(Mod, "WingTime", Language.GetTextValue(LocalizationRoot + "wingFlyTimeLine.Unit", flyTime)),
            "Equipable", "ItemName");
    }

    /// <summary>Returns the added line, or null if the item doesn't use ammo.</summary>
    private TooltipLine AddAmmoLine(Item item, Item ammo, List<TooltipLine> lines)
    {
        lines.Find(l => l.Name == "NeedsBait")?.Hide();
        lines.Find(l => l.Name == "WandConsumes")?.Hide();

        if (ammo == null && item.useAmmo <= 0 && item.fishingPole <= 0 && item.tileWand <= 0)
            return null;

        var ammoLine = new TooltipLine(Mod, "AmmoLine", ammo?.HoverName ?? MissingAmmoText(item));
        if (ammo != null)
            ammoLine.SetColor(RarityColor(ammo.rare));

        lines.Insert(lines.FindLastIndex(l => LinesBeforeAmmo.Contains(l.Name)) + 1, ammoLine);
        return ammoLine;
    }

    private static string MissingAmmoText(Item item)
    {
        string ammoName = item.fishingPole > 0
            ? Language.GetTextValue(LocalizationRoot + "ammoLine.Bait")
            : AmmoTypeNames.TryGetValue(item.useAmmo, out string key)
                ? Language.GetTextValue(LocalizationRoot + "ammoLine." + key)
                : Lang.GetItemNameValue(item.useAmmo > 0 ? item.useAmmo : item.tileWand);

        return Language.GetTextValue(LocalizationRoot + "ammoLine.No") + ammoName;
    }

    private void ReplacePriceLine(Item item, List<TooltipLine> lines)
    {
        TooltipLine specialPrice = lines.Find(l => l.Name == "SpecialPrice");
        specialPrice?.Hide();
        lines.Find(l => l.Name == "Price")?.Hide();

        if (item.IsACoin)
            return;

        long price = PriceOfStack(item);
        if (price <= 0)
            return;

        string text = item.shopSpecialCurrency >= 0 ? SpecialPriceText(item, specialPrice) : CoinPriceText(price);
        if (text != "")
            lines.Add(new TooltipLine(Mod, "PriceLine", text));
    }

    private static string SpecialPriceText(Item item, TooltipLine specialPrice)
    {
        if (!item.buy || specialPrice == null)
            return "";

        // Drop the leading "Buy price:".
        return specialPrice.Text.Replace(Language.GetTextValue("LegacyTooltip.50"), "").TrimStart();
    }

    private static string CoinPriceText(long price)
    {
        (long amount, Color color, int name)[] coins =
        {
            (price / 1_000_000, Colors.CoinPlatinum, 15),
            (price / 10_000 % 100, Colors.CoinGold, 16),
            (price / 100 % 100, Colors.CoinSilver, 17),
            (price % 100, Colors.CoinCopper, 18),
        };

        float pulse = Main.mouseTextColor / 255f;
        return string.Concat(coins
            .Where(c => c.amount > 0)
            .Select(c => $"[c/{(c.color * pulse).Hex3()}:{c.amount} {Lang.inter[c.name].Value} ]"));
    }

    private static long PriceOfStack(Item item)
    {
        if (item.value == 0 && !item.shopCustomPrice.HasValue)
            return 0;

        Player.GetItemExpectedPrice(item, out long sellPrice, out long buyPrice);
        if (item.buy)
            return item.stack * buyPrice;

        long sellPerItem = (item.isAShopItem || item.buyOnce ? buyPrice : sellPrice) / 5;
        long total = sellPerItem * item.stack;

        // Items just sold to a shop can be bought back at full price.
        int sellback = Main.shopSellbackHelper.GetAmount(item);
        if (sellback > 0)
            total += (buyPrice - sellPerItem) * Math.Min(sellback, item.stack);

        return total;
    }

    private void AddModName(Item item, Item ammo, TooltipLine ammoLine, List<TooltipLine> lines, Config config)
    {
        if (config.modNameNextToItemName)
        {
            if (ammoLine != null && ammo?.ModItem != null)
                ammoLine.Text += " - " + ammo.ModItem.Mod.DisplayName;

            TooltipLine name = lines.Find(l => l.Name == "ItemName");
            if (name != null && item.ModItem != null)
                name.Text += " - " + item.ModItem.Mod.DisplayName;
        }
        else if (config.modNameColor.A != 0 && item.ModItem != null)
        {
            lines.Add(new TooltipLine(Mod, "ModName", item.ModItem.Mod.DisplayName));
        }
    }

    private static void ImproveDamageLine(Item item, Item ammo, List<TooltipLine> lines, Config config)
    {
        TooltipLine damage = lines.Find(l => l.Name == "Damage");
        if (damage == null || ammo == null || !config.wpnPlusAmmoDmg)
            return;

        int total = Player.GetWeaponDamage(item) + Player.GetWeaponDamage(ammo);
        int numberEnd = damage.Text.IndexOf(' ');
        if (numberEnd >= 0)
            damage.Text = total + damage.Text[numberEnd..];
    }

    private static void ImproveKnockbackLine(Item item, Item ammo, List<TooltipLine> lines, Config config)
    {
        TooltipLine knockback = lines.Find(l => l.Name == "Knockback");
        if (knockback == null)
            return;

        // The game adds ammo knockback before applying the player's bonuses, so this matches what actually happens.
        float baseKnockback = item.knockBack + (ammo != null && config.wpnPlusAmmoKb ? ammo.knockBack : 0);
        float total = Player.GetWeaponKnockback(item, baseKnockback);

        // LegacyTooltip.45 is "% knockback", the percent sign is dropped.
        knockback.Text = Math.Round(total, 2) + Language.GetTextValue("LegacyTooltip.45")[1..];
    }

    private static void ImproveSpeedLine(Item item, List<TooltipLine> lines)
    {
        TooltipLine speed = lines.Find(l => l.Name == "Speed");
        if (speed == null)
            return;

        // The reuse delay starts once the use animation ends, so one attack takes both.
        int animationTicks = CombinedHooks.TotalAnimationTime(item.useAnimation, Player, item);
        float delayTicks = item.reuseDelay / CombinedHooks.TotalUseSpeedMultiplier(Player, item);
        double attacksPerSecond = Math.Round(60f / (animationTicks + delayTicks), 2);

        speed.Text = attacksPerSecond + Language.GetTextValue(LocalizationRoot + "speedLine.Display");
    }

    /// <summary>Alpha 0 hides a line. White means keep the color the line already has.</summary>
    private static void ApplyLineColors(List<TooltipLine> lines, Config config)
    {
        foreach (TooltipLine line in lines)
        {
            Color? configured = line.IsBadPrefix() ? config.badMod
                : line.IsGoodPrefix() ? config.goodMod
                : config.LineColor(line.Name);

            if (configured is not Color color)
                continue;

            if (color.A == 0)
                line.Hide();
            else if (color != Color.White)
                line.SetColor(color);
        }
    }

    /// <summary>Inserts after the first line that exists out of <paramref name="anchors"/>, or at the top if none do.</summary>
    private static void InsertAfterFirstFound(List<TooltipLine> lines, TooltipLine newLine, params string[] anchors)
    {
        foreach (string anchor in anchors)
        {
            int index = lines.FindIndex(l => l.Name == anchor);
            if (index >= 0)
            {
                lines.Insert(index + 1, newLine);
                return;
            }
        }

        lines.Insert(0, newLine);
    }

    private static Color RarityColor(int rare)
    {
        if (rare >= ItemRarityID.Count)
            return RarityLoader.GetRarity(rare)?.RarityColor ?? Color.White;

        return ItemRarity.GetColor(rare);
    }
}
