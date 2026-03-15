using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEnums;
using UnityEngine;

namespace TemplateMod
{
    public class JumpkickAttack : Attack
    {
        private Agent target;

        public override AttackEnum AttackEnum => (AttackEnum)1013;

        public override string LocalizationTableKey { get; } = "Jumpkick";

        public override int InitialValue => -1;

        public override int InitialCooldown => 3;

        public override int[] Range { get; protected set; } = new int[1] { 3 };

        public override string AnimationTrigger { get; protected set; } = "DashForward";

        public override AttackEffectEnum[] CompatibleEffects { get; protected set; } = new AttackEffectEnum[0];

        public override void Initialize(int maxLevel)
        {
            base.Initialize(maxLevel);
            base.TileEffect = TileEffectEnum.FreePlay;
        }

        private Cell landingCell; // Keep the reference to the landing cell so movement doesn't need to look for it again
        public override bool Begin(Agent attacker)
        {
            base.Begin(attacker); // Run the default Begin function
            landingCell = Attacker.Cell.Neighbour(Attacker.FacingDir, 2); // Get the cell 2 ahead of where we're standing
            if(landingCell == null || landingCell.Agent != null) // If this goes beyond the battlefield, or the cell is occupied
            {
                return false; // Tile will fail.
            }
            StartCoroutine(PerformAttack()); // Run the tile's effects manually - necessary for non-damaging tiles
            return true;
        }

        private IEnumerator PerformAttack()
        {
            Cell hitCell = landingCell.Neighbour(Attacker.FacingDir, 1); // Get the tile to potentially hit with rotation

            Vector3 hitPoint = hitCell ? (landingCell.transform.position + hitCell.transform.position) / 2f : landingCell.transform.position;
            yield return StartCoroutine(Dash(base.Attacker.transform.position, hitPoint, 15)); // Visually move the attacker a little ahead of the landing tile
            Attacker.Cell = landingCell; // Actually move the attacker to the landing tile
            if(hitCell && hitCell.Agent && hitCell.Agent.Movable) // If there's a movable agent in the cell in front of us...
            {
                FlipTarget(hitCell.Agent); // ...turn them around. FlipTarget taken unchanged from BoAttack
                yield return new WaitForSeconds(Agent.turnAroundTime);
            }

            Attacker.SetIdleAnimation(value: true);
            yield return StartCoroutine(base.Attacker.MoveToCoroutine(hitPoint, landingCell.transform.position, .2f)); // Reset the attacker's visuals to be centered on the cell
            if (base.Attacker == Globals.Hero)
            {
                EventsManager.Instance.HeroPerformedMoveAttack.Invoke(); // Notify the game we moved with a tile - used by skills
            }
            base.Attacker.AttackInProgress = false;
        }

        private IEnumerator Dash(Vector3 from, Vector3 to, float speed) //copied unchanged from ChargeAttack
        {
            SoundEffectsManager.Instance.Play("Dash");
            float time = Vector3.Distance(from, to) / speed;
            yield return StartCoroutine(base.Attacker.MoveToCoroutine(from, to, time, 0f, createDustEffect: true, createDashEffect: true));
        }

        private void FlipTarget(Agent target)
        {
            if (target.AgentStats.ice == 0)
            {
                target.RegisterActionInProgress(Agent.turnAroundTime);
                target.TurnAround();
            }
            else
            {
                target.Flip();
            }
        }
    }
}
