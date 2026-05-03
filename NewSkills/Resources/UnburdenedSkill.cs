using CombatEnums;
using ContentLoader.HelperFunctions;
using System.Collections;
using UnityEngine;
using SkillEnums;

namespace TemplateMod
{
    public class UnburdenedSkill : Skill
    {
        public override SkillEnum SkillEnum { get; } = (SkillEnum)4004;

        public override int MaxLevel { get; protected set; } = 1;

        public override string LocalizationTableKey { get; } = "Unburdened";

        private bool justUsedPotion = false;

        public override void PickUp()
        {
            base.PickUp();
            PotionsManager.Instance.NPotionsSlots--;
            EventsManager.Instance.PotionUsed.AddListener(OnConsumableUsed);
            EventHelper.OnBeforePlayerAction += TestForFreeAction;
        }

        public override void Remove()
        {
            base.Remove();
            PotionsManager.Instance.NPotionsSlots++;
            EventsManager.Instance.PotionUsed.RemoveListener(OnConsumableUsed);
            EventHelper.OnBeforePlayerAction -= TestForFreeAction;
        }

        private void OnConsumableUsed(Potion consumable)
        {
            justUsedPotion = true;

            InvokeSkillTriggeredEvent();
        }

        private bool TestForFreeAction(ActionEnum action)
        {
            if (!justUsedPotion) return true;
            justUsedPotion = false;
            Globals.Hero.Action = action;

            // Hero action handling copied from CombatManager.
            if (Globals.Hero.Action == ActionEnum.Wait)
            {
                Globals.Hero.Wait();
            }
            if (Globals.Hero.Action == ActionEnum.Attack)
            {
                Globals.Hero.ExecuteAttacksInQueue();
            }
            if (Globals.Hero.ActionIsMove)
            {
                Globals.Hero.StartCoroutine(Globals.Hero.PerformMoveAction(Globals.Hero.MoveDir));
            }
            if (Globals.Hero.Action == ActionEnum.FlipLeft)
            {
                Globals.Hero.FlipLeft();
            }
            if (Globals.Hero.Action == ActionEnum.FlipRight)
            {
                Globals.Hero.FlipRight();
            }

            return false;
        }
    }
}
