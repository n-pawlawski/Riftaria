using System.Collections.Generic;
using System.Linq;

namespace Rifthold.Common.Maps
{
	/// <summary>
	/// The tier table. Three tiers is enough to prove the loop closes: clearing
	/// one has to be able to hand you the key to the next.
	/// </summary>
	public static class RiftTierRegistry
	{
		private static readonly Dictionary<int, RiftTier> Tiers = new();

		public const int FirstTier = 1;

		public static int MaxTier => Tiers.Count == 0 ? FirstTier : Tiers.Keys.Max();

		public static IEnumerable<RiftTier> All => Tiers.Values.OrderBy(tier => tier.Tier);

		public static void Register(RiftTier tier) => Tiers[tier.Tier] = tier;

		public static RiftTier Get(int tier)
		{
			if (Tiers.TryGetValue(tier, out RiftTier found)) {
				return found;
			}

			// Defensive: an unknown tier should still produce a playable rift
			// rather than throwing part-way through world generation.
			return Tiers.TryGetValue(FirstTier, out RiftTier fallback) ? fallback : Default(tier);
		}

		public static bool Exists(int tier) => Tiers.ContainsKey(tier);

		internal static void LoadDefaults()
		{
			Register(new RiftTier {
				Tier = 1,
				RequiredLevel = 1,
				RoomCount = 5,
				EnemyScaling = 1f,
				EnemyBudget = 6,
				CompletionXp = 220,
				EssenceReward = 4,
				NextTierSigilChance = 0.5f
			});

			Register(new RiftTier {
				Tier = 2,
				RequiredLevel = 5,
				RoomCount = 7,
				EnemyScaling = 1.6f,
				EnemyBudget = 8,
				CompletionXp = 520,
				EssenceReward = 7,
				NextTierSigilChance = 0.4f
			});

			Register(new RiftTier {
				Tier = 3,
				RequiredLevel = 10,
				RoomCount = 9,
				EnemyScaling = 2.4f,
				EnemyBudget = 10,
				CompletionXp = 1100,
				EssenceReward = 11,
				NextTierSigilChance = 0f
			});
		}

		private static RiftTier Default(int tier) => new() {
			Tier = tier,
			RequiredLevel = 1,
			RoomCount = 5,
			EnemyScaling = 1f,
			EnemyBudget = 6,
			CompletionXp = 200,
			EssenceReward = 3,
			NextTierSigilChance = 0f
		};

		internal static void Reset() => Tiers.Clear();
	}
}
