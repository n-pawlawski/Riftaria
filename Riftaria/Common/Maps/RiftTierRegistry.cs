using System.Collections.Generic;
using System.Linq;
using Riftaria.Common.World;

namespace Riftaria.Common.Maps
{
	/// <summary>
	/// The tier table. The design calls for ten, one per section of the vanilla game.
	/// Three are enough for the first playable: clearing one has to open the next, and
	/// you need a third to prove that is a rule rather than a special case.
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
			Register(new RiftTier { Tier = 1, RoomCount = 5, EnemyScaling = 1f, EnemyBudget = 6, Biome = BiomeId.Forest });
			Register(new RiftTier { Tier = 2, RoomCount = 7, EnemyScaling = 1.6f, EnemyBudget = 8, Biome = BiomeId.Evil });
			Register(new RiftTier { Tier = 3, RoomCount = 9, EnemyScaling = 2.4f, EnemyBudget = 10, Biome = BiomeId.Stone });
		}

		private static RiftTier Default(int tier) => new() {
			Tier = tier,
			RoomCount = 5,
			EnemyScaling = 1f,
			EnemyBudget = 6,
			Biome = BiomeId.Stone
		};

		internal static void Reset() => Tiers.Clear();
	}
}
