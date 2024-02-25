namespace TrueTooltips
{
    using Microsoft.Xna.Framework;
    using System.ComponentModel;
    using Terraria.ModLoader.Config;

    class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("CustomTooltipLines")]
        [DefaultValue(true)]
        public bool ammoLine;

        [DefaultValue(true)]
        public bool priceLine;

        [DefaultValue(true)]
        public bool speedLine;

        [DefaultValue(true)]
        public bool knockbackLine;

        [DefaultValue(true)]
        public bool wpnPlusAmmoDmg;

        [DefaultValue(true)]
        public bool wpnPlusAmmoKb;

        [DefaultValue(true)]
        public bool wpnPlusAmmoVelocity;

        [DefaultValue(true)]
        public bool modName;

        [Header("Background")]

        [DefaultValue(typeof(Color), "63,81,151,255")]
        public Color bgColor;

        [DefaultValue(typeof(Color), "63,81,151,255")]
        public Color spritebgColor;

        [Range(0, 999),
        DefaultValue(10)]
        public int paddingRight;

        [Range(0, 999),
        DefaultValue(10)]
        public int paddingLeft;

        [Range(0, 999),
        DefaultValue(10)]
        public int paddingTop;

        [Range(0, 999),
        DefaultValue(10)]
        public int paddingBottom;

        [Header("Offset")]

        [Range(0, 999),
        DefaultValue(16)]
        public int x;

        [Range(0, 999),
        DefaultValue(16)]
        public int y;

        [Range(-15, 999),
        DefaultValue(0)]
        public int spacing;

        [Header("ItemSprite")]

        [DefaultValue(true)]
        public bool sprite;

        [DefaultValue(true)]
        public bool spriteBorder;

        [Range(0, 999),
        DefaultValue(10)]
        public int spriteBorderPadding;

        [Range(1,999),
        DefaultValue(30)]
        public int spriteMin;

        [Range(0, 999),
        DefaultValue(10)]
        public int spriteTextPadding;

        [Header("Miscellaneous")]

        [DefaultValue(true)]
        public bool textPulse;

        [DefaultValue(false)]
        public bool ammoCrit;

        [Header("VanillaTooltipLines")]

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color tileBoost;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color ammo;

        [DefaultValue(typeof(Color), "190,120,120,255")]
        public Color badMod;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color placeable;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color consumable;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color wandConsumes;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color dmg;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color equipable;

        [DefaultValue(typeof(Color), "255,255,255,0")]
        public Color social;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color etherianMana;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color expert;

        [DefaultValue(typeof(Color), "255,255,255,0")]
        public Color favDescr;

        [DefaultValue(typeof(Color), "255,214,0,0")]
        public Color fav;
        
        [DefaultValue(typeof(Color), "120,190,120,255")]
        public Color goodMod;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color wellFedExpert;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color knockback;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color material;

        [DefaultValue(typeof(Color), "255,255,255,0")]
        public Color quest;

        [DefaultValue(typeof(Color), "255,255,255,0")]
        public Color needsBait;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color healLife;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color healMana;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color setBonus;

        [DefaultValue(typeof(Color), "255,255,255,0")]
        public Color socialDescr;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color speed;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color useMana;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color vanity;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color defense;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color buffTime;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color axePow;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color baitPow;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color critChance;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color fishingPow;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color velocityLine;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color hammerPow;

        [DefaultValue(typeof(Color), "255,255,255,255")]
        public Color pickPow;
        
    }
}