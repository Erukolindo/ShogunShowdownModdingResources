using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEnums;
using UnityEngine;
using Utils;

namespace TemplateMod
{
    public class MomentumChargeAttack : Attack
    {
        public override AttackEnum AttackEnum => (AttackEnum)1016;

        public override string LocalizationTableKey { get; } = "MomentumCharge";

        public override int InitialValue => 0;

        public override int InitialCooldown => 6;

        public override int[] Range { get; protected set; } = Attack.InfiniteForwardRange;

        public override string AnimationTrigger { get; protected set; } = "DashForward";

        public override AttackEffectEnum[] CompatibleEffects { get; protected set; } = new AttackEffectEnum[6]
        {
        AttackEffectEnum.Ice,
        AttackEffectEnum.DoubleStrike,
        AttackEffectEnum.Shockwave,
        AttackEffectEnum.Poison,
        AttackEffectEnum.PerfectStrike,
        AttackEffectEnum.Curse
        };

        private readonly float speed = 15f;

        private readonly float bounceTime = 0.2f;

        private readonly float hitAnimationTime = 0.025f;

        private int distanceMoved;

        protected RelativeDir RelativeChargeDirection { get; }

        private Dir Direction
        {
            get
            {
                if (RelativeChargeDirection == RelativeDir.Forward)
                {
                    return base.Attacker.FacingDir;
                }
                return DirUtils.Opposite(base.Attacker.FacingDir);
            }
        }

        public override bool Begin(Agent attacker)
        {
            base.Begin(attacker);
            Cell cell = base.Attacker.Cell.LastFreeCellInDirection(Direction);
            Cell cell2 = cell.Neighbour(Direction, 1);
            bool flag = cell2 != null && cell2.Agent != null;
            if (cell == base.Attacker.Cell && !flag)
            {
                return false;
            }
            base.Attacker.AttackInProgress = true;
            base.Attacker.SetIdleAnimation(value: false);
            if (flag)
            {
                StartCoroutine(DashAndHit(cell, cell2));
            }
            else
            {
                StartCoroutine(DashOnly(cell));
            }
            return true;
        }

        private IEnumerator DashOnly(Cell targetMoveCell)
        {
            yield return StartCoroutine(Dash(base.Attacker.transform.position, targetMoveCell.transform.position, speed));
            base.Attacker.Cell = targetMoveCell;
            if (base.Attacker == Globals.Hero)
            {
                EventsManager.Instance.HeroPerformedMoveAttack.Invoke();
            }
            base.Attacker.SetIdleAnimation(value: true);
            base.Attacker.AttackInProgress = false;
        }

        private IEnumerator DashAndHit(Cell targetMoveCell, Cell targetHitCell)
        {
            distanceMoved = Mathf.Abs(Attacker.Cell.IndexInGrid - targetMoveCell.IndexInGrid);
            Value += distanceMoved;
            bool attackerMovesToDifferentCell = base.Attacker.Cell != targetMoveCell;
            Vector3 hitPoint = (targetMoveCell.transform.position + targetHitCell.transform.position) / 2f;
            yield return StartCoroutine(Dash(base.Attacker.transform.position, hitPoint, speed));
            base.Attacker.Cell = targetMoveCell;
            HitTarget(targetHitCell.Agent);
            yield return new WaitForSeconds(hitAnimationTime);
            base.Attacker.SetIdleAnimation(value: true);
            yield return StartCoroutine(base.Attacker.MoveToCoroutine(hitPoint, targetMoveCell.transform.position, bounceTime));
            if (base.Attacker == Globals.Hero && attackerMovesToDifferentCell)
            {
                EventsManager.Instance.HeroPerformedMoveAttack.Invoke();
            }
            Value -= distanceMoved;
            base.Attacker.AttackInProgress = false;
        }

        private IEnumerator Dash(Vector3 from, Vector3 to, float speed)
        {
            SoundEffectsManager.Instance.Play("Dash");
            float time = Vector3.Distance(from, to) / speed;
            yield return StartCoroutine(base.Attacker.MoveToCoroutine(from, to, time, 0f, createDustEffect: true, createDashEffect: true));
        }
    }
}
