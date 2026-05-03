using ContentLoader.HelperFunctions;
using SkillEnums;
using System.Collections.Generic;
using UnityEngine;

namespace TemplateMod
{
    public class EclipticVigorSkill : Skill
    {
        public override SkillEnum SkillEnum { get; } = (SkillEnum)3006;

        public override int MaxLevel { get; protected set; } = 1;

        public override string LocalizationTableKey { get; } = "EclipticVigor";

        private bool skipNextDoubling = false;

        public override void PickUp()
        {
            base.PickUp();
            // The challenge is complete upon the end of a boss fight
            EventsManager.Instance.EndBossFight.AddListener(CompleteBet);
            int pastTriggers = ModSaveManager.Get<int>("EclipticVigorTriggers", 0, ModSaveManager.StorageScope.Run);
            if (pastTriggers == 0)
            {
                Enemy_Awake_Patch.OnEnemyAwake += BoostEnemyHealth;
            }
            else
            {
                EventsManager.Instance.HeroHPUpdateDetailed.AddListener(DoubleHealing);
            }
        }

        public override void Remove()
        {
            EventsManager.Instance.EndBossFight.RemoveListener(CompleteBet);
            int pastTriggers = ModSaveManager.Get<int>("EclipticVigorTriggers", 0, ModSaveManager.StorageScope.Run);
            if (pastTriggers == 0)
            {
                Enemy_Awake_Patch.OnEnemyAwake -= BoostEnemyHealth;
            }
            else
            {
                EventsManager.Instance.HeroHPUpdateDetailed.RemoveListener(DoubleHealing);
            }
                base.Remove();
        }

        private void BoostEnemyHealth(Enemy enemy)
        {
            if(enemy is Boss) return;
            enemy.AddToMaxHealth(1);
        }

        private void DoubleHealing((int requestedDeltaHP, int actualDeltaHP, int heroHP) hpDetails)
        {
            if (hpDetails.requestedDeltaHP <= 0) return;
            if(skipNextDoubling)
            {
                skipNextDoubling = false;
                return;
            }
            skipNextDoubling = true;
            Globals.Hero.AddToHealth(hpDetails.requestedDeltaHP);
        }

        private void CompleteBet()
        {
            // Make sure this is the first time the completion is being triggered
            int pastTriggers = ModSaveManager.Get<int>("EclipticVigorTriggers", 0, ModSaveManager.StorageScope.Run);

            if (pastTriggers == 0)
            {
                InvokeSkillTriggeredEvent();
                ModSaveManager.Set<int>("EclipticVigorTriggers", 1, ModSaveManager.StorageScope.Run);
                Enemy_Awake_Patch.OnEnemyAwake -= BoostEnemyHealth;
                EventsManager.Instance.HeroHPUpdateDetailed.AddListener(DoubleHealing);
            }
        }
    }
}
