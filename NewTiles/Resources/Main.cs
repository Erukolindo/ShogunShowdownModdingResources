using HarmonyLib;
using UnityModManagerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ContentLoader.DataClasses;
using ContentLoader;
using TileEnums;

namespace TemplateMod
{
    public class Main
    {
        private static List<TileData> attacksToLoad = new List<TileData>
        {
            new TileData("Jumpkick", 1013, -1013, typeof(JumpkickAttack), SOHandlingEnum.Generate),
            new TileData("WavyBlade", 1014, -1014, typeof(WavyBladeAttack), SOHandlingEnum.Generate),
            new TileData("BigMon", 1015, -1015, typeof(BigMonAttack), SOHandlingEnum.Generate),
            new TileData("MomentumCharge", 1016, -1016, typeof(MomentumChargeAttack), SOHandlingEnum.Generate),
        };

        private static Dictionary<(string table, string key), string> stringsToLoad = new Dictionary<(string, string), string>()
        {
            { ("TileAttacks", "Jumpkick_Description"), "Leap 2 cells ahead, then turn around the target directly in front of you." },
            { ("TileAttacks", "Jumpkick_Name"), "Jumpkick" },
            { ("TileAttacks", "WavyBlade_Description"), "Strike the nearest target in the first two cells ahead." },
            { ("TileAttacks", "WavyBlade_Name"), "Wavy Blade" },
            { ("TileAttacks", "BigMon_Description"), "Strike the first target ahead and push them away.\nSpend 5 coins!" },
            { ("TileAttacks", "BigMon_Name"), "Big Mon" },
            { ("TileAttacks", "MomentumCharge_Description"), "Dash forward and strike the first target ahead. Increase damage by 1 for every cell dashed through." },
            { ("TileAttacks", "MomentumCharge_Name"), "Momentum Charge" },
        };

        private static Dictionary<AttackEnum, List<TagEnum>> defaultTileTags = new Dictionary<AttackEnum, List<TagEnum>>()
        {
            { (AttackEnum)1013, new List<TagEnum> { TagEnum.Martial, TagEnum.NonDamaging, TagEnum.Rotating, TagEnum.MovesUser } },
            { (AttackEnum)1014, new List<TagEnum> { TagEnum.Slash, TagEnum.Weapon } },
            { (AttackEnum)1015, new List<TagEnum> { TagEnum.Blunt, TagEnum.Weapon, TagEnum.Projectile, TagEnum.Knockback, TagEnum.ExtraCost } },
            { (AttackEnum)1016, new List<TagEnum> { TagEnum.Blunt, TagEnum.Martial, TagEnum.DashLike, TagEnum.MovesUser } },
        };

        static AssetBundle bundle = null;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var bundlePath = System.IO.Path.Combine(modEntry.Path, "templatemod");
            bundle = AssetBundle.LoadFromFile(bundlePath);

            Dictionary<string, object> contentData = new Dictionary<string, object>();
            contentData["stringsToLoad"] = stringsToLoad;
            contentData["attacksToLoad"] = attacksToLoad;
            contentData["tileTags"] = defaultTileTags;

            if (!ContentLoader.Main.LoadContent("TemplateMod", contentData, bundle)) return false;

            var harmony = new Harmony("com.erukolindo.templatemod");
            harmony.PatchAll();

            return true;
        }
    }
}
