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
using UnlocksID;

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

        private static List<SkillData> skillsToLoad = new List<SkillData>
        {
            new SkillData("ComboMight", 2009, -2033, typeof(ComboMightSkill), SOHandlingEnum.Generate, SOHandlingEnum.Generate, 20),
            new SkillData("Whetstone", 1002, -2034, typeof(WhetstoneSkill), SOHandlingEnum.Ignore, SOHandlingEnum.Generate, 20),
            new SkillData("Unburdened", 4004, -2035, typeof(UnburdenedSkill), SOHandlingEnum.Generate, SOHandlingEnum.Generate, 20),
            new SkillData("EclipticVigor", 3006, -2036, typeof(EclipticVigorSkill), SOHandlingEnum.Generate, SOHandlingEnum.Generate, 15),
        };

        private static List<Func<Quest>> questsToLoad = new List<Func<Quest>>
        {
            AllOrNothingQuest,
            OutsmartedQuest
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
            { ("Skills", "ComboMight_Name"), "Combo Might" },
            { ("Skills", "ComboMight_Description"), "Increase the damage of all attacks in your next queue by {0} when executing a combo kill." },
            { ("Skills", "Whetstone_Name"), "Whetstone" },
            { ("Skills", "Whetstone_Description"), "Apply a +1 damage upgrade to two random tiles." },
            { ("Skills", "Unburdened_Name"), "Unburdened" },
            { ("Skills", "Unburdened_Description"), "Lose a consumable slot. After using a consumable, your next action is free." },
            { ("Skills", "EclipticVigor_Name"), "Ecliptic Vigor" },
            { ("Skills", "EclipticVigor_Description"), "All enemies in the next region gain +1 Max Health. Double all healing received past that region." },
            { ("Metaprogression", "AllOrNothing_Name"), "All or Nothing" },
            { ("Metaprogression", "AllOrNothing_Description"), "Defeat The Shogun with a deck containing only tiles with 0 or 8 cooldown." },
            { ("Metaprogression", "Outsmarted_Name"), "Outsmarted" },
            { ("Metaprogression", "Outsmarted_Description"), "Have an enemy kill Hideyoshi the Cunning." },
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
            contentData["skillsToLoad"] = skillsToLoad;
            contentData["questsToLoad"] = questsToLoad;

            if (!ContentLoader.Main.LoadContent("TemplateMod", contentData, bundle)) return false;

            var harmony = new Harmony("com.erukolindo.templatemod");
            harmony.PatchAll();

            return true;
        }

        private static Quest AllOrNothingQuest()
        {
            AllTilesCooldownValueQuest quest = ScriptableObject.CreateInstance<AllTilesCooldownValueQuest>();
            AccessTools.Field(typeof(Quest), "LocalizationTableKey").SetValue(quest, "AllOrNothing");
            quest.unlockID = (UnlockID)(-3005);
            quest.additionalUnlocks = new UnlockID[1] { (UnlockID)(-2034) };
            quest.requiredUnlocksForUnveiled = new UnlockID[0];
            quest.symbolNotCompleted = bundle.LoadAsset<Sprite>("QuestSymbols_AllOrNothing_no");
            quest.symbolCompleted = bundle.LoadAsset<Sprite>("QuestSymbols_AllOrNothing_yes");

            AccessTools.Field(typeof(AllTilesCooldownValueQuest), "cooldownValues").SetValue(quest, new int[2] { 0,8 });
            return quest;
        }

        private static Quest OutsmartedQuest()
        {
            OutsmartedQuest quest = ScriptableObject.CreateInstance<OutsmartedQuest>();
            AccessTools.Field(typeof(Quest), "LocalizationTableKey").SetValue(quest, "Outsmarted");
            quest.unlockID = (UnlockID)(-3006);
            quest.additionalUnlocks = new UnlockID[0];
            quest.requiredUnlocksForUnveiled = new UnlockID[1] { UnlockID.q_daimyo_1_defeated };
            quest.symbolNotCompleted = bundle.LoadAsset<Sprite>("QuestSymbols_Outsmarted_no");
            quest.symbolCompleted = bundle.LoadAsset<Sprite>("QuestSymbols_Outsmarted_yes");

            return quest;
        }
    }
}
