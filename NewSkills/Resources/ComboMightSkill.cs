using System.Collections.Generic;
using SkillEnums;
using Utils;
using ContentLoader.HelperFunctions;

namespace TemplateMod
{
    public class ComboMightSkill : Skill
    {
        public override SkillEnum SkillEnum { get; } = (SkillEnum)2009;

        public override int MaxLevel { get; protected set; } = 2;

        public override string LocalizationTableKey { get; } = "ComboMight";

        private int ExtraDamage => base.Level;

        private int currentBonusDamage = 0;
        private int bonusDamageBuffer = 0;
        private bool bonusDamageUsed = false;

        protected override string ProcessDescription(string description)
        {
            return string.Format(description, ExtraDamage);
        }

        public override void PickUp()
        {
            base.PickUp();
            // Subscribe to events
            EventsManager.Instance.ComboKill.AddListener(ComboKill);
            EventsManager.Instance.Attack.AddListener(ProcessAttack);
            EventsManager.Instance.EndOfCombatTurn.AddListener(EndOfCombatTurn);
            EventsManager.Instance.EndOfCombat.AddListener(EndOfCombat);

            // Load saved bonus damage if it exists
            currentBonusDamage = ModSaveManager.Get("ComboMight_currentBonusDamage", 0, ModSaveManager.StorageScope.Run);
        }

        private void ComboKill(Enemy enemy)
        {
            // Add the extra damage to the buffer
            bonusDamageBuffer += ExtraDamage;
            InvokeSkillTriggeredEvent();
        }

        private void ProcessAttack(Agent attacker, Agent defender, Hit hit)
        {
            // Standard conditions for applying the bonus damage: the attacker must be the hero and the hit must synergize with skills
            if (!(attacker != Globals.Hero) && hit.SynergizeWithSkills)
            {
                hit.Damage += currentBonusDamage;
                SoundEffectsManager.Instance.Play("SpecialHit");
                InvokeSkillTriggeredEvent();
                // Set a flag to reset the bonus damage after this turn
                bonusDamageUsed = true;
            }
        }

        private void EndOfCombatTurn()
        {
            // Reset the bonus damage if it was used
            if(bonusDamageUsed) currentBonusDamage = 0;
            // Add any buffered bonus damage from combo kills that occurred during this turn
            currentBonusDamage += bonusDamageBuffer;
            // Save the current bonus damage in case the player closes the game before the next turn
            ModSaveManager.Set("ComboMight_currentBonusDamage", currentBonusDamage, ModSaveManager.StorageScope.Run);
            // Reset the flag and buffer for the next turn
            bonusDamageUsed = false;
            bonusDamageBuffer = 0;
        }

        private void EndOfCombat()
        {
            // Reset everything at the end of combat
            currentBonusDamage = 0;
            ModSaveManager.Set("ComboMight_currentBonusDamage", 0, ModSaveManager.StorageScope.Run);
            bonusDamageBuffer = 0;
            bonusDamageUsed = false;
        }

        public override void Remove()
        {
            base.Remove();
            EventsManager.Instance.ComboKill.RemoveListener(ComboKill);
            EventsManager.Instance.Attack.RemoveListener(ProcessAttack);
            EventsManager.Instance.EndOfCombatTurn.RemoveListener(EndOfCombatTurn);
            EventsManager.Instance.EndOfCombat.RemoveListener(EndOfCombat);
        }
    }
}
