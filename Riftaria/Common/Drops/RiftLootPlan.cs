using System;
using System.Collections.Generic;
using Riftaria.Common.Items;

namespace Riftaria.Common.Drops
{
	/// <summary>A base and rarity, ready for <see cref="ItemRoller"/>.</summary>
	public sealed class LootPick
	{
		public LootBase Base { get; init; }

		public Rarity Rarity { get; init; }
	}

	public sealed class GeneralPool
	{
		public float DropChance { get; set; }

		public List<LootBase> Bases { get; set; } = new();

		public RarityWeights Rarity { get; set; } = new();
	}

	/// <summary>
	/// What a boss drops. A boss bag stores one of these, so the bag still knows
	/// its rift's bases after the rift is gone.
	/// </summary>
	public sealed class BossPool
	{
		public int Tier { get; set; }

		public int Items { get; set; }

		public List<LootBase> Bases { get; set; } = new();

		public RarityWeights Rarity { get; set; } = new();
	}

	/// <summary>
	/// The loot of one rift, built once when it generates. Each rift draws its
	/// own subset of bases, so two runs of the same tier drop different things.
	/// tools/rift-preview/item-roller.js is a copy of this and has to be changed
	/// with it.
	/// </summary>
	public sealed class RiftLootPlan
	{
		public int Tier { get; set; }

		public GeneralPool General { get; set; } = new();

		public BossPool Boss { get; set; } = new();

		public List<BossOverride> Overrides { get; set; } = new();

		public static RiftLootPlan Build(LootFile file, int tier, Func<int, int> next)
		{
			var plan = new RiftLootPlan { Tier = tier };
			LootTierDef def = LootTable.FindTier(file, tier);
			if (def == null) {
				return plan;
			}

			var eligible = new List<LootBase>();
			foreach (LootBase entry in file.Bases) {
				if (entry.DropsIn(tier)) {
					eligible.Add(entry);
				}
			}

			plan.General = new GeneralPool {
				DropChance = def.General.DropChance,
				Bases = Draw(eligible, def.General.BaseCount, next),
				Rarity = def.General.Rarity.Copy()
			};
			plan.Boss = new BossPool {
				Tier = tier,
				Items = def.Boss.Items,
				Bases = Draw(eligible, def.Boss.BaseCount, next),
				Rarity = def.Boss.Rarity.Copy()
			};
			plan.Overrides = new List<BossOverride>(file.Bosses);
			return plan;
		}

		/// <summary>Null when the kill rolls no drop.</summary>
		public LootPick RollGeneral(Func<int, int> next)
		{
			if (General.Bases.Count == 0 || General.DropChance <= 0f) {
				return null;
			}

			const int Scale = 1_000_000;
			if (next(Scale) >= (int)Math.Round(General.DropChance * Scale)) {
				return null;
			}

			return Pick(General.Bases, General.Rarity, next);
		}

		/// <summary>
		/// The boss pool with this boss's overrides applied. An override for this
		/// exact tier wins over one for every tier.
		/// </summary>
		public BossPool BossFor(string bossId)
		{
			var pool = new BossPool {
				Tier = Boss.Tier,
				Items = Boss.Items,
				Bases = new List<LootBase>(Boss.Bases),
				Rarity = Boss.Rarity.Copy()
			};

			BossOverride any = null;
			BossOverride exact = null;
			foreach (BossOverride entry in Overrides) {
				if (!string.Equals(entry.Boss, bossId, StringComparison.Ordinal)) {
					continue;
				}

				if (entry.Tier == Tier) {
					exact = entry;
				}
				else if (entry.Tier == 0) {
					any = entry;
				}
			}

			Apply(pool, any);
			Apply(pool, exact);
			return pool;
		}

		public static List<LootPick> RollBoss(BossPool pool, Func<int, int> next)
		{
			var picks = new List<LootPick>();
			for (int i = 0; i < pool.Items; i++) {
				LootPick pick = Pick(pool.Bases, pool.Rarity, next);
				if (pick != null) {
					picks.Add(pick);
				}
			}

			return picks;
		}

		public static LootPick Pick(List<LootBase> bases, RarityWeights rarity, Func<int, int> next)
		{
			int index = LootTable.Pick(bases, entry => entry.Weight, next);
			if (index < 0) {
				return null;
			}

			return new LootPick { Base = bases[index], Rarity = rarity.Roll(next) };
		}

		private static void Apply(BossPool pool, BossOverride entry)
		{
			if (entry == null) {
				return;
			}

			if (entry.Items.HasValue) {
				pool.Items = entry.Items.Value;
			}

			if (entry.Rarity != null) {
				pool.Rarity = entry.Rarity.Copy();
			}
		}

		/// <summary>Weighted picks without repeats. A count of 0, or more than there are, keeps them all.</summary>
		private static List<LootBase> Draw(List<LootBase> eligible, int count, Func<int, int> next)
		{
			if (count <= 0 || count >= eligible.Count) {
				return new List<LootBase>(eligible);
			}

			var left = new List<LootBase>(eligible);
			var drawn = new List<LootBase>(count);
			while (drawn.Count < count) {
				int index = LootTable.Pick(left, entry => entry.Weight, next);
				if (index < 0) {
					break;
				}

				drawn.Add(left[index]);
				left.RemoveAt(index);
			}

			return drawn;
		}
	}
}
