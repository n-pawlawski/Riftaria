using System;
using System.Collections.Generic;

namespace Riftaria.Common.Items
{
	/// <summary>
	/// Rolls affixes onto a base. No Terraria types, so DropCheck runs the same
	/// roll the game does. tools/rift-preview/item-roller.js is a copy of this
	/// and has to be changed with it.
	/// <para>
	/// <paramref name="next"/> on every method returns an int in [0, max), the
	/// same shape as the game's random roll.
	/// </para>
	/// </summary>
	public static class ItemRoller
	{
		private static readonly List<AffixCount> DefaultMagic = new() {
			new AffixCount { Count = 1, Weight = 60 },
			new AffixCount { Count = 2, Weight = 40 }
		};

		private static readonly List<AffixCount> DefaultRare = new() {
			new AffixCount { Count = 3, Weight = 30 },
			new AffixCount { Count = 4, Weight = 35 },
			new AffixCount { Count = 5, Weight = 25 },
			new AffixCount { Count = 6, Weight = 10 }
		};

		public static RolledItem RollItem(AffixFile file, string item, string tag, int level, Rarity rarity, string baseName, Func<int, int> next)
		{
			List<RolledAffix> affixes = RollAffixes(file, new[] { tag }, level, rarity, next);
			return new RolledItem {
				Item = item,
				Tag = tag,
				Level = level,
				Rarity = rarity,
				Affixes = affixes,
				Name = Compose(file, rarity, affixes, baseName, next)
			};
		}

		/// <summary>
		/// Picks a target count for the rarity, then adds one affix at a time.
		/// An affix's chance is the summed weight of the tiers the level unlocks,
		/// so higher tiers make the affix itself more common, as in PoE. The roll
		/// stops early when nothing else fits.
		/// </summary>
		public static List<RolledAffix> RollAffixes(AffixFile file, IReadOnlyCollection<string> tags, int level, Rarity rarity, Func<int, int> next)
		{
			var result = new List<RolledAffix>();
			if (rarity == Rarity.Normal || file?.Affixes == null) {
				return result;
			}

			int target = TargetCount(file, rarity, next);
			int maxPrefixes = RarityRules.MaxPrefixes(rarity);
			int maxSuffixes = RarityRules.MaxSuffixes(rarity);
			int prefixes = 0;
			int suffixes = 0;
			var groups = new HashSet<string>(StringComparer.Ordinal);
			var candidates = new List<AffixDef>();
			var weights = new List<int>();

			while (result.Count < target) {
				candidates.Clear();
				weights.Clear();
				int total = 0;
				foreach (AffixDef affix in file.Affixes) {
					if (affix.IsPrefix ? prefixes >= maxPrefixes : suffixes >= maxSuffixes) {
						continue;
					}

					if (groups.Contains(affix.GroupKey) || !SharesTag(affix, tags)) {
						continue;
					}

					int weight = UnlockedWeight(affix, level);
					if (weight <= 0) {
						continue;
					}

					candidates.Add(affix);
					weights.Add(weight);
					total += weight;
				}

				if (total <= 0) {
					break;
				}

				int index = PickIndex(weights, total, next);
				AffixDef chosen = candidates[index];
				result.Add(RollTier(chosen, level, weights[index], next));
				groups.Add(chosen.GroupKey);
				if (chosen.IsPrefix) {
					prefixes++;
				}
				else {
					suffixes++;
				}
			}

			var ordered = new List<RolledAffix>(result.Count);
			ordered.AddRange(result.FindAll(affix => affix.Prefix));
			ordered.AddRange(result.FindAll(affix => !affix.Prefix));
			return ordered;
		}

		/// <summary>Magic reads "Prefix Base Suffix". Rare gets two random words.</summary>
		public static string Compose(AffixFile file, Rarity rarity, IReadOnlyList<RolledAffix> affixes, string baseName, Func<int, int> next)
		{
			if (rarity == Rarity.Rare) {
				List<string> first = file?.RareNames?.First;
				List<string> second = file?.RareNames?.Second;
				if (first == null || second == null || first.Count == 0 || second.Count == 0) {
					return baseName;
				}

				return first[next(first.Count)] + " " + second[next(second.Count)];
			}

			if (rarity != Rarity.Magic) {
				return baseName;
			}

			string prefix = "";
			string suffix = "";
			foreach (RolledAffix rolled in affixes) {
				string name = AffixTable.FindAffix(file, rolled.Id)?.Name ?? "";
				if (rolled.Prefix) {
					prefix = name;
				}
				else {
					suffix = name;
				}
			}

			var parts = new List<string>(3);
			if (prefix.Length > 0) {
				parts.Add(prefix);
			}

			parts.Add(baseName);
			if (suffix.Length > 0) {
				parts.Add(suffix);
			}

			return string.Join(" ", parts);
		}

		public static int UnlockedWeight(AffixDef affix, int level)
		{
			int total = 0;
			foreach (AffixTier tier in affix.Tiers) {
				if (tier.MinLevel <= level && tier.Weight > 0) {
					total += tier.Weight;
				}
			}

			return total;
		}

		private static int TargetCount(AffixFile file, Rarity rarity, Func<int, int> next)
		{
			List<AffixCount> counts = rarity == Rarity.Magic ? file.Rarities?.Magic : file.Rarities?.Rare;
			int total = Positive(counts);
			if (total <= 0) {
				counts = rarity == Rarity.Magic ? DefaultMagic : DefaultRare;
				total = Positive(counts);
			}

			int ticket = next(total);
			int cursor = 0;
			foreach (AffixCount count in counts) {
				if (count.Weight <= 0) {
					continue;
				}

				cursor += count.Weight;
				if (ticket < cursor) {
					return count.Count;
				}
			}

			return 0;
		}

		private static int Positive(List<AffixCount> counts)
		{
			int total = 0;
			if (counts == null) {
				return 0;
			}

			foreach (AffixCount count in counts) {
				if (count.Weight > 0) {
					total += count.Weight;
				}
			}

			return total;
		}

		private static bool SharesTag(AffixDef affix, IReadOnlyCollection<string> tags)
		{
			foreach (string tag in affix.Tags) {
				foreach (string want in tags) {
					if (string.Equals(tag, want, StringComparison.Ordinal)) {
						return true;
					}
				}
			}

			return false;
		}

		private static int PickIndex(List<int> weights, int total, Func<int, int> next)
		{
			int ticket = next(total);
			int cursor = 0;
			for (int i = 0; i < weights.Count; i++) {
				cursor += weights[i];
				if (ticket < cursor) {
					return i;
				}
			}

			return weights.Count - 1;
		}

		private static RolledAffix RollTier(AffixDef affix, int level, int unlocked, Func<int, int> next)
		{
			int ticket = next(unlocked);
			int cursor = 0;
			int index = 0;
			for (int i = 0; i < affix.Tiers.Count; i++) {
				AffixTier tier = affix.Tiers[i];
				if (tier.MinLevel > level || tier.Weight <= 0) {
					continue;
				}

				index = i;
				cursor += tier.Weight;
				if (ticket < cursor) {
					break;
				}
			}

			AffixTier picked = affix.Tiers[index];
			return new RolledAffix {
				Id = affix.Id,
				Prefix = affix.IsPrefix,
				Stat = affix.Stat,
				Tier = affix.Tiers.Count - index,
				Value = picked.Min + next(Math.Max(1, picked.Max - picked.Min + 1))
			};
		}
	}
}
