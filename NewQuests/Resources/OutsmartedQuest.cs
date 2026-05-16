using UnityEngine;

namespace TemplateMod
{
    public class OutsmartedQuest : Quest
    {
        private bool hideyoshiTakesEnemyDamage = false;

        public override void Initialize()
        {
            EventsManager.Instance.BossDied.AddListener(BossDied);
            EventsManager.Instance.Attack.AddListener(ProcessAttack);
        }

        public override void FinalizeQuest()
        {
            EventsManager.Instance.BossDied.RemoveListener(BossDied);
            EventsManager.Instance.Attack.RemoveListener(ProcessAttack);
        }

        private void BossDied(Boss boss)
        {
            if (hideyoshiTakesEnemyDamage)
            {
                QuestCompleted();
            }
        }

        private void ProcessAttack(Agent attacker, Agent defender, Hit hit)
        {
            hideyoshiTakesEnemyDamage = !(attacker == null) && !hit.IsCollision && attacker is Enemy enemy && defender is HideyoshiBoss && !enemy.Inanimate;
        }
    }
}
