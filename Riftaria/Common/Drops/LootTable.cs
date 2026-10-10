using System;
using System.Collections.Generic;
using Riftaria.Common.Items;

namespace Riftaria.Common.Drops
{
	/// <summary>
	/// A vanilla item that affixes can roll onto. <see cref="Tag"/> decides which
	/// affixes fit, and <see cref="MaxTier"/> of 0 means no upper limit.
	/// </summary>
	public sealed class LootBase
	{
		public string Item { get; set; } = "";

		public string Tag { get; set; } = "";

		public int MinTier { get; set; } = 1;

		public int MaxTier { get; set; }

		public int Weight { get; set; } = 100;

		public bool DropsIn(int tier) => MinTier <= tier && (MaxTier <= 0 || tier <= MaxTier) && Weight > 0;
	}

	public sealed class RarityWeights
	{
		public int Normal { get; set; }

		public int Magic { get; set; }

		public int Rare { get; set; }

		public int Total => Math.Max(0, Normal) + Math.Max(0, Magic) + Math.Max(0, Rare);

		/// <summary>An empty table rolls Normal.</summary>
		public Rarity Roll(Func<int, int> next)
		{
			int total = Total;
			if (total <= 0) {
				return Rarity.Normal;
			}

			int ticket = next(total);
			if (ticket < Math.Max(0, Normal)) {
				return Rarity.Normal;
			}

			ticket -= Math.Max(0, Normal);
			return ticket < Math.Max(0, Magic) ? Rarity.Magic : Rarity.Rare;
		}

		public RarityWeights Copy() => new() { Normal = Normal, Magic = Magic, Rare = Rare };
	}

	/// <summary>Loot from enemy kills, and later chests.</summary>
	public sealed class GeneralPoolDef
	{
		/// <summary>Chance per kill, from 0 to 1.</summary>
		public float DropChance { get; set; }

		/// <summary>How many bases one rift picks. 0 keeps every base that fits the tier.</summary>
		public int BaseCount { get; set; }

		public RarityWeights Rarity { get; set; } = new();
	}

	public sealed class BossPoolDef
	{
		public int Items { get; set; } = 1;

		public int BaseCount { get; set; }

		public RarityWeights Rarity { get; set; } = new();
	}

	public sealed class LootTierDef
	{
		public int Tier { get; set; }

		public GeneralPoolDef General { get; set; } = new();

		public BossPoolDef Boss { get; set; } = new();
	}

	/// <summary>
	/// Replaces parts of the boss pool for one boss. Tier 0 applies to every
	/// tier. Unset fields keep the tier's value.
	/// </summary>
	public sealed class BossOverride
	{
		public string Boss { get; set; } = "";

		public int Tier { get; set; }

		public int? Items { get; set; }

		public RarityWeights Rarity { get; set; }
	}

	public sealed class LootFile
	{
		public List<LootBase> Bases { get; set; } = new();

		public List<LootTierDef> Tiers { get; set; } = new();

		public List<BossOverride> Bosses { get; set; } = new();
	}

	/// <summary>
	/// Reads loot-pools.js. No Terraria types, so a script can run the same roll
	/// the game uses.
	/// </summary>
	public static class LootTable
	{
		public static LootFile Parse(string text)
		{
			LootFile file = DataJson.Read<LootFile>(text);
			file.Bases ??= new List<LootBase>();
			file.Tiers ??= new List<LootTierDef>();
			file.Bosses ??= new List<BossOverride>();
			foreach (LootBase entry in file.Bases) {
				entry.Item ??= "";
				entry.Tag ??= "";
			}

			foreach (LootTierDef tier in file.Tiers) {
				tier.General ??= new GeneralPoolDef();
				tier.General.Rarity ??= new RarityWeights();
				tier.Boss ??= new BossPoolDef();
				tier.Boss.Rarity ??= new RarityWeights();
			}

			foreach (BossOverride boss in file.Bosses) {
				boss.Boss ??= "";
			}

			return file;
		}

		/// <summary>The exact tier, or the highest tier below it, or the lowest tier defined.</summary>
		public static LootTierDef FindTier(LootFile file, int tier)
		{
			LootTierDef below = null;
			LootTierDef lowest = null;
			foreach (LootTierDef entry in file?.Tiers ?? new List<LootTierDef>()) {
				if (entry.Tier == tier) {
					return entry;
				}

				if (entry.Tier < tier && (below == null || entry.Tier > below.Tier)) {
					below = entry;
				}

				if (lowest == null || entry.Tier < lowest.Tier) {
					lowest = entry;
				}
			}

			return below ?? lowest;
		}

		/// <summary>One weighted pick. Returns -1 when nothing has weight.</summary>
		public static int Pick<T>(IReadOnlyList<T> items, Func<T, int> weight, Func<int, int> next)
		{
			int total = 0;
			foreach (T item in items) {
				total += Math.Max(0, weight(item));
			}

			if (total <= 0) {
				return -1;
			}

			int ticket = next(total);
			int cursor = 0;
			for (int i = 0; i < items.Count; i++) {
				int w = weight(items[i]);
				if (w <= 0) {
					continue;
				}

				cursor += w;
				if (ticket < cursor) {
					return i;
				}
			}

			return -1;
		}

		/// <summary>The preview page runs the same checks before it saves.</summary>
		public static List<string> Validate(LootFile file, AffixFile affixes)
		{
			var errors = new List<string>();
			var tags = new HashSet<string>(affixes?.Tags ?? new List<string>(), StringComparer.Ordinal);
			foreach (LootBase entry in file.Bases) {
				string who = string.IsNullOrEmpty(entry.Item) ? "A base" : "Base " + entry.Item;
				if (string.IsNullOrEmpty(entry.Item)) {
					errors.Add("A base has no item.");
				}

				if (!tags.Contains(entry.Tag)) {
					errors.Add(who + " uses unknown tag " + entry.Tag + ".");
				}

				if (entry.MaxTier > 0 && entry.MaxTier < entry.MinTier) {
					errors.Add(who + " has a max tier below its min tier.");
				}

				if (entry.Weight < 0) {
					errors.Add(who + " has a negative weight.");
				}
			}

			var seen = new HashSet<int>();
			foreach (LootTierDef tier in file.Tiers) {
				if (!seen.Add(tier.Tier)) {
					errors.Add("Tier " + tier.Tier + " is listed twice.");
				}

				if (tier.General.DropChance < 0f || tier.General.DropChance > 1f) {
					errors.Add("Tier " + tier.Tier + " drop chance must be between 0 and 1.");
				}

				if (tier.General.BaseCount < 0 || tier.Boss.BaseCount < 0 || tier.Boss.Items < 0) {
					errors.Add("Tier " + tier.Tier + " has a negative count.");
				}

				bool any = false;
				foreach (LootBase entry in file.Bases) {
					any |= entry.DropsIn(tier.Tier);
				}

				if (!any) {
					errors.Add("Tier " + tier.Tier + " has no bases that can drop.");
				}
			}

			foreach (BossOverride boss in file.Bosses) {
				if (string.IsNullOrEmpty(boss.Boss)) {
					errors.Add("A boss override has no boss.");
				}

				if (boss.Items < 0) {
					errors.Add("Boss " + boss.Boss + " has a negative item count.");
				}
			}

			return errors;
		}
	}
}
