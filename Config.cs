namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using System.ComponentModel;
    using Terraria.ModLoader.Config;

    class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("[c/ffff00:Custom Tooltip Lines]")]

        [Label("Ammo Line"),
        Tooltip("Shows the item's ammo's name, amount and rarity, shows the mod that adds the ammo if \"Mod name next to item name\" is on and shows the ammo the item needs if the item has no ammo."),
        DefaultValue(true)]
        public bool ammoLine;

        [Label("Better Price Line"),
        Tooltip("Correct sell price for items with custom price, always visible, better color coding, cleaner. Replaces vanilla price line."),
        DefaultValue(true)]
        public bool priceLine;

        [Label("Better Speed Line"),
        Tooltip("Shows attacks per second. Replaces vanilla speed line."),
        DefaultValue(true)]
        public bool speedLine;

        [Label("Better Knockback Line"),
        Tooltip("Shows knockback as a number. Replaces vanilla knockback line."),
        DefaultValue(true)]
        public bool knockbackLine;

        [Label("Combine weapon and ammo damage"), DefaultValue(true)]
        public bool wpnPlusAmmoDmg;

        [Label("Combine weapon and ammo knockback"),
        Tooltip("Requires Better Knockback Line to be on."),
        DefaultValue(true)]
        public bool wpnPlusAmmoKb;

        [Label("Mod name next to item name"),
        Tooltip("Shows the mod that adds the item next to the item's name."),
        DefaultValue(true)]
        public bool modName;

        [Header("[c/ffff00:Background Color]")]

        [Label(""), DefaultValue(typeof(Color), "63,81,151,255")]
        public Color bgColor;

        [Header("[c/ffff00:Miscellaneous]")]

        [Label("Text Pulse"), DefaultValue(false)]
        public bool textPulse;

        [Label("Crit chance line for ammo"),
        Tooltip("Off by default because ammo crit chance doesn't affect crit chance."),
        DefaultValue(false)]
        public bool ammoCrit;

        [Header("[c/ffff00:Vanilla Tooltip Lines]")]

        [Label("+- X range"),
        Tooltip("How much more/less range the tool has, in tiles."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color tileBoost;

        [Label("Ammo"),
        Tooltip("The item is ammo."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color ammo;

        [Label("Bad Modifier"), DefaultValue(typeof(Color), "190,120,120,255")]
        public Color badMod;

        [Label("Can be placed"),
        Tooltip("The item can be placed."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color placeable;

        [Label("Consumable"),
        Tooltip("The item is consumable."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color consumable;

        [Label("Consumes X"),
        Tooltip("What item a tile wand consumes."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color wandConsumes;

        [Label("Damage"),
        Tooltip("The item's damage and class."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color dmg;

        [Label("Equipable"),
        Tooltip("The item is equipable."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color equipable;

        [Label("Equipped in social slot"),
        Tooltip("The item is in a social slot."),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color social;

        [Label("Etherian Mana Warning"),
        Tooltip("\"Cannot be used without Etherian Mana until the Eternia Crystal has been defended\""),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color etherianMana;

        [Label("Expert"),
        Tooltip("The item is from Expert Mode."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color expert;

        [Label("Favorite Description"),
        Tooltip("\"Quick trash, stacking, and selling will be blocked\""),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color favDescr;

        [Label("Good Modifier"), DefaultValue(typeof(Color), "120,190,120,255")]
        public Color goodMod;

        [Label("Increases life regeneration"),
        Tooltip("In Expert Mode, shows that the food increases life regeneration."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color wellFedExpert;

        [Label("Knockback"),
        Tooltip("The item's knockback."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color knockback;

        [Label("Marked as favorite"),
        Tooltip("The item is marked as favorite."),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color fav;

        [Label("Material"),
        Tooltip("The item can be used to craft something."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color material;

        [Label("Quest Item"),
        Tooltip("The item is a quest item."),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color quest;

        [Label("Requires bait to catch fish"),
        Tooltip("The fishing pole requires bait to catch fish."),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color needsBait;

        [Label("Restores X life"),
        Tooltip("How much life the player restores when using the item."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color healLife;

        [Label("Restores X mana"),
        Tooltip("How much mana the player restores when using the item."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color healMana;

        [Label("Set Bonus"),
        Tooltip("The armor's set bonus description."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color setBonus;

        [Label("Social Description"),
        Tooltip("\"No stats will be gained\""),
        DefaultValue(typeof(Color), "255,255,255,0")]
        public Color socialDescr;

        [Label("Speed"),
        Tooltip("The item's attack speed."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color speed;

        [Label("Uses X mana"),
        Tooltip("How much mana the item consumes when used."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color useMana;

        [Label("Vanity Item"),
        Tooltip("The item is vanity."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color vanity;

        [Label("X defense"),
        Tooltip("The item's defense."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color defense;

        [Label("X minute duration"),
        Tooltip("The item's buff duration."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color buffTime;

        [Label("X% axe power"),
        Tooltip("The item's axe power."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color axePow;

        [Label("X% bait power"),
        Tooltip("The bait's bait power."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color baitPow;

        [Label("X% critical strike chance"),
        Tooltip("The item's crit chance."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color critChance;

        [Label("X% fishing power"),
        Tooltip("The fishing pole's fishing power."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color fishingPow;

        [Label("X% hammer power"),
        Tooltip("The item's hammer power."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color hammerPow;

        [Label("X% pickaxe power"),
        Tooltip("The item's pickaxe power."),
        DefaultValue(typeof(Color), "255,255,255,255")]
        public Color pickPow;
    }
}