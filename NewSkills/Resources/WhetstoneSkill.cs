using SkillEnums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ContentLoader.HelperFunctions;

namespace TemplateMod
{
    public class WhetstoneSkill : Skill
    {
        public override SkillEnum SkillEnum { get; } = (SkillEnum)1002;

        public override int MaxLevel { get; protected set; } = 5;

        public override string LocalizationTableKey { get; } = "Whetstone";

        public override void LevelUp()
        {
            base.LevelUp();
            // if the level we're reaching is greater than the number of times we've used the skill this run, apply the upgrade.
            if (base.CurrentlyHeld && ModSaveManager.Get("Whetstone_uses", 0, ModSaveManager.StorageScope.Run) < Level)
            {
                ApplyUpgrade();
            }
        }

        public override void PickUp()
        {
            base.PickUp();
            // If we're actually picking up the skill for the first time (as opposed to loading it from a save), apply the upgrade
            if (ModSaveManager.Get("Whetstone_uses", 0, ModSaveManager.StorageScope.Run) == 0)
            {
                ApplyUpgrade();
            }
        }

        private void ApplyUpgrade()
        {
            // Get a list of hero's tiles
            List<Tile> heroTiles = TilesManager.Instance.hand.TCC.Tiles.ToList();
            // Filter out: tiles without an attack value, tiles without an upgrade slot, and tiles that already have maximum damage
            // We iterate backwards through the list to be able to safely remove tiles from it
            for (int i = heroTiles.Count-1; i > -1; i--)
            {
                bool hasAttackValue = heroTiles[i].Attack.HasValue;
                bool hasUpgradeSlot = heroTiles[i].Attack.Level < heroTiles[i].Attack.MaxLevel;
                bool notMaxDamage = heroTiles[i].Attack.Value < Attack.maxValue;
                if (!hasAttackValue || !hasUpgradeSlot || !notMaxDamage)
                {
                    heroTiles.RemoveAt(i);
                }
            }

            // If there are no viable tiles, do nothing
            if (heroTiles.Count == 0)
            {
                return;
            }

            // Give 2 randomly selected viable tiles a +1 damage upgrade
            for (int i = 0; i < 2; i++)
            {
                int randomIndex = Random.Range(0, heroTiles.Count);
                Tile tile = heroTiles[randomIndex];
                tile.Attack.Value++;
                tile.Attack.BaseValue++;
                tile.Attack.Level++;
                tile.Graphics.UpdateGraphics();
                if (tile.Attack.Level == tile.Attack.MaxLevel)
                {
                    heroTiles.RemoveAt(randomIndex);
                    // If there was only one viable tile, don't try to upgrade a second one
                    if (heroTiles.Count == 0)
                    {
                        break;
                    }
                }
            }

            // Count the uses of the skill this run and save it
            ModSaveManager.Set("Whetstone_uses", ModSaveManager.Get("Whetstone_uses", 0, ModSaveManager.StorageScope.Run) + 1, ModSaveManager.StorageScope.Run);
        }

        public override void Remove()
        {
            base.Remove();
            // In case player re-obtained the skill after losing it, we need to reset the number of uses this run
            ModSaveManager.Set("Whetstone_uses", 0, ModSaveManager.StorageScope.Run);
        }
    }
}
