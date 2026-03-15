using System.Collections;
using TileEnums;
using UnityEngine;
using Utils;

namespace TemplateMod
{
    public class BigMonAttack : Attack
    {
        public override AttackEnum AttackEnum => (AttackEnum)1015;

        public override string LocalizationTableKey => "BigMon";

        public override int InitialValue => 7;

        public override int InitialCooldown => 6;

        public override int[] Range { get; protected set; } = Attack.InfiniteForwardRange;

        public override string AnimationTrigger { get; protected set; } = "ProjectileThrow";

        public override AttackEffectEnum[] CompatibleEffects { get; protected set; } = new AttackEffectEnum[6]
        {
        AttackEffectEnum.Ice,
        AttackEffectEnum.DoubleStrike,
        AttackEffectEnum.Shockwave,
        AttackEffectEnum.Poison,
        AttackEffectEnum.PerfectStrike,
        AttackEffectEnum.Curse
        };

        private bool pushInProgress;

        public override bool Begin(Agent attacker)
        {
            base.Begin(attacker);
            if (base.Attacker is Hero && Globals.Coins <= 4)
            {
                return false;
            }
            StartCoroutine(PerformAttack());
            return true;
        }

        private IEnumerator PerformAttack()
        {
            base.Attacker.AttackInProgress = true;
            if (base.Attacker is Hero)
            {
                Globals.Coins-=5;
            }
            Agent target = AgentInRange(base.Attacker);
            SoundEffectsManager.Instance.Play("MoneySpent");
            yield return new WaitForSeconds(0.1f);
            ProjectileEffect projectile = EffectsManager.Instance.CreateInGameEffect("MonProjectileEffect", base.Attacker.transform.position).GetComponent<ProjectileEffect>();
            Vector3 targetPosition = ((target != null) ? target.transform.position : (base.Attacker.transform.position + 20f * DirUtils.ToVec(base.Attacker.FacingDir)));
            projectile.Throw(base.Attacker.transform.position, targetPosition);
            if (target != null)
            {
                while (projectile.MovingTowardsTargetPosition)
                {
                    yield return null;
                }
                SoundEffectsManager.Instance.Play("MonHit");
                HitTarget(target);
                if (target.Movable)
                {
                    pushInProgress = true;
                    yield return StartCoroutine(target.Pushed(base.Attacker.FacingDir));
                    pushInProgress = false;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(0.1f);
            base.Attacker.AttackInProgress = false;
        }

        public override bool WaitingForSomethingToFinish()
        {
            return pushInProgress;
        }
    }
}
