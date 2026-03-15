using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEnums;

namespace TemplateMod
{
    public class WavyBladeAttack : Attack
    {
        public override AttackEnum AttackEnum
        {
            get
            {
                return (AttackEnum)1014;
            }
        }

        public override string LocalizationTableKey
        {
            get
            {
                return "WavyBlade";
            }
        }

        public override int InitialValue
        {
            get
            {
                return 3;
            }
        }

        public override int InitialCooldown
        {
            get
            {
                return 4;
            }
        }

        public override int[] Range { get; protected set; } = new int[]
        {
        1,
        2
        };

        public override string AnimationTrigger { get; protected set; } = "KatanaAttack";

        public override AttackEffectEnum[] CompatibleEffects { get; protected set; } = new AttackEffectEnum[]
        {
        AttackEffectEnum.Ice,
        AttackEffectEnum.DoubleStrike,
        AttackEffectEnum.Shockwave,
        AttackEffectEnum.Poison,
        AttackEffectEnum.PerfectStrike,
        AttackEffectEnum.Curse
        };

        protected override bool ClosestTargetOnly { get; set; } = true;

        public override void ApplyEffect()
        {
            Agent[] array = this.AgentsInRange(base.Attacker);
            if (array.Length == 0)
            {
                SoundEffectsManager.Instance.Play("MissHit");
                return;
            }
            foreach (Agent target in array)
            {
                base.HitTarget(target, "CombatHit");
            }
        }
    }
}
