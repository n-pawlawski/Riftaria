using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// True while the boss is alive and its life is strictly under a fraction
	/// of its max. Pass 0.5f for half health.
	/// </summary>
	public sealed class HealthBelow : IBossCondition
	{
		private readonly float fraction;

		public HealthBelow(float fraction)
		{
			this.fraction = fraction;
		}

		public bool IsMet(BossFight fight)
		{
			NPC boss = fight.Boss;
			if (boss.lifeMax <= 0 || boss.life <= 0) {
				return false;
			}

			return (float)boss.life / boss.lifeMax < fraction;
		}
	}
}
