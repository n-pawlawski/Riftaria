using Terraria;
using Terraria.ModLoader;

namespace Riftaria.Content.Prefixes
{
	public enum RiftBowKind
	{
		Damage,
		DamageAndSpeed,
		Loaded
	}

	/// <summary>
	/// Prefixes already sitting on bows from before the loot file. New bags do
	/// not apply these. The roll lives on the item itself.
	/// </summary>
	public abstract class RiftBowPrefix : ModPrefix
	{
		public abstract RiftBowKind Kind { get; }

		public override PrefixCategory Category => PrefixCategory.Ranged;

		/// <summary>
		/// Weight in a random reforge. Zero keeps these off the tinkerer.
		/// </summary>
		public override float RollChance(Item item) => 0f;

		public override bool CanRoll(Item item) => item.DamageType.CountsAsClass(DamageClass.Ranged);

		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
		{
			damageMult = 1.25f;
			if (Kind == RiftBowKind.Damage) {
				return;
			}

			useTimeMult = 1f / 1.10f;
			if (Kind == RiftBowKind.Loaded) {
				critBonus = 10;
				shootSpeedMult = 1.20f;
			}
		}
	}

	public class HeavystringBow : RiftBowPrefix
	{
		public override RiftBowKind Kind => RiftBowKind.Damage;
	}

	public class QuickstringBow : RiftBowPrefix
	{
		public override RiftBowKind Kind => RiftBowKind.DamageAndSpeed;
	}

	public class RiftforgedBow : RiftBowPrefix
	{
		public override RiftBowKind Kind => RiftBowKind.Loaded;
	}
}
