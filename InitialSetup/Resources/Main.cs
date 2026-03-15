using HarmonyLib;
using UnityModManagerNet;
using UnityEngine;
using ContentLoader;
using ContentLoader.DataClasses;
using TileEnums;

namespace TemplateMod
{
    public class Main
    {
        static AssetBundle bundle = null;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var bundlePath = System.IO.Path.Combine(modEntry.Path, "templatemod");
            bundle = AssetBundle.LoadFromFile(bundlePath);

            Dictionary<string, object> contentData = new Dictionary<string, object>();

            if (!ContentLoader.Main.LoadContent("TemplateMod", contentData, bundle)) return false;

            var harmony = new Harmony("com.erukolindo.templatemod");
            harmony.PatchAll();

            return true;
        }
    }
}
