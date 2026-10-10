using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Riftaria.Common.Drops;

namespace Riftaria.Common.Items
{
	/// <summary>
	/// A stat as the data file describes it. The label holds a # where the
	/// rolled value goes, such as "#% increased damage".
	/// </summary>
	public sealed class StatDef
	{
		public string Id { get; set; } = "";

		public string Label { get; set; } = "";

		/// <summary>"flat" or "percent".</summary>
		public string Format { get; set; } = "flat";

		/// <summary>"local" or "global".</summary>
		public string Scope { get; set; } = "global";
	}

	/// <summary>A value range unlocked once the item level reaches <see cref="MinLevel"/>.</summary>
	public sealed class AffixTier
	{
		public int MinLevel { get; set; } = 1;

		public int Min { get; set; }

		public int Max { get; set; }

		public int Weight { get; set; }
	}

	public sealed class AffixDef
	{
		public string Id { get; set; } = "";

		/// <summary>The word used in a Magic item's name, such as "Heavy" or "of Haste".</summary>
		public string Name { get; set; } = "";

		/// <summary>"prefix" or "suffix".</summary>
		public string Kind { get; set; } = "prefix";

		/// <summary>An item holds at most one affix from each group. Empty means the id.</summary>
		public string Group { get; set; } = "";

		public string Stat { get; set; } = "";

		public List<string> Tags { get; set; } = new();

		/// <summary>Sorted by <see cref="AffixTier.MinLevel"/>, so the last tier is tier 1.</summary>
		public List<AffixTier> Tiers { get; set; } = new();

		[JsonIgnore]
		public bool IsPrefix => string.Equals(Kind, "prefix", StringComparison.OrdinalIgnoreCase);

		[JsonIgnore]
		public string GroupKey => string.IsNullOrEmpty(Group) ? Id : Group;
	}

	public sealed class AffixCount
	{
		public int Count { get; set; }

		public int Weight { get; set; }
	}

	public sealed class RarityRolls
	{
		public List<AffixCount> Magic { get; set; } = new();

		public List<AffixCount> Rare { get; set; } = new();
	}

	public sealed class RareNames
	{
		public List<string> First { get; set; } = new();

		public List<string> Second { get; set; } = new();
	}

	public sealed class AffixFile
	{
		public List<string> Tags { get; set; } = new();

		public List<StatDef> Stats { get; set; } = new();

		/// <summary>How many affixes each rarity aims for, by weight.</summary>
		public RarityRolls Rarities { get; set; } = new();

		public RareNames RareNames { get; set; } = new();

		public List<AffixDef> Affixes { get; set; } = new();
	}

	/// <summary>Reads and checks affixes.js. No Terraria types, so a script can use it.</summary>
	public static class AffixTable
	{
		public static AffixFile Parse(string text)
		{
			AffixFile file = DataJson.Read<AffixFile>(text);
			file.Tags ??= new List<string>();
			file.Stats ??= new List<StatDef>();
			file.Rarities ??= new RarityRolls();
			file.Rarities.Magic ??= new List<AffixCount>();
			file.Rarities.Rare ??= new List<AffixCount>();
			file.RareNames ??= new RareNames();
			file.RareNames.First ??= new List<string>();
			file.RareNames.Second ??= new List<string>();
			file.Affixes ??= new List<AffixDef>();
			foreach (AffixDef affix in file.Affixes) {
				affix.Id ??= "";
				affix.Name ??= "";
				affix.Kind ??= "prefix";
				affix.Group ??= "";
				affix.Stat ??= "";
				affix.Tags ??= new List<string>();
				affix.Tiers ??= new List<AffixTier>();
			}

			return file;
		}

		public static AffixDef FindAffix(AffixFile file, string id)
		{
			if (file?.Affixes == null) {
				return null;
			}

			foreach (AffixDef affix in file.Affixes) {
				if (string.Equals(affix.Id, id, StringComparison.Ordinal)) {
					return affix;
				}
			}

			return null;
		}

		public static StatDef FindStat(AffixFile file, string id)
		{
			if (file?.Stats == null) {
				return null;
			}

			foreach (StatDef stat in file.Stats) {
				if (string.Equals(stat.Id, id, StringComparison.Ordinal)) {
					return stat;
				}
			}

			return null;
		}

		/// <summary>The tooltip line for one affix, such as "+12% increased damage".</summary>
		public static string Describe(AffixFile file, RolledAffix affix)
		{
			StatDef stat = FindStat(file, affix.Stat);
			string label = string.IsNullOrEmpty(stat?.Label) ? affix.Stat + " #" : stat.Label;
			return label.Replace("#", affix.Value.ToString());
		}

		/// <summary>
		/// Problems that would make a roll wrong. The preview page runs the same
		/// checks before it saves.
		/// </summary>
		public static List<string> Validate(AffixFile file)
		{
			var errors = new List<string>();
			var tags = new HashSet<string>(file.Tags, StringComparer.Ordinal);
			var stats = new HashSet<string>(StringComparer.Ordinal);
			foreach (StatDef stat in file.Stats) {
				if (!stats.Add(stat.Id)) {
					errors.Add("Stat " + stat.Id + " is listed twice.");
				}

				if (!Stats.TryParse(stat.Id, out _)) {
					errors.Add("Stat " + stat.Id + " is not a stat the game knows.");
				}
			}

			var ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (AffixDef affix in file.Affixes) {
				string who = string.IsNullOrEmpty(affix.Id) ? "An affix" : affix.Id;
				if (string.IsNullOrEmpty(affix.Id)) {
					errors.Add("An affix has no id.");
				}
				else if (!ids.Add(affix.Id)) {
					errors.Add(who + " is listed twice.");
				}

				if (affix.Kind != "prefix" && affix.Kind != "suffix") {
					errors.Add(who + " must be a prefix or a suffix.");
				}

				if (!stats.Contains(affix.Stat)) {
					errors.Add(who + " uses unknown stat " + affix.Stat + ".");
				}

				if (affix.Tags.Count == 0) {
					errors.Add(who + " has no tags, so it can never roll.");
				}

				foreach (string tag in affix.Tags) {
					if (!tags.Contains(tag)) {
						errors.Add(who + " uses unknown tag " + tag + ".");
					}
				}

				if (affix.Tiers.Count == 0) {
					errors.Add(who + " has no tiers.");
				}

				int lastLevel = int.MinValue;
				for (int i = 0; i < affix.Tiers.Count; i++) {
					AffixTier tier = affix.Tiers[i];
					string where = who + " tier row " + (i + 1);
					if (tier.Min > tier.Max) {
						errors.Add(where + " has min above max.");
					}

					if (tier.Weight < 0) {
						errors.Add(where + " has a negative weight.");
					}

					if (tier.MinLevel < 1) {
						errors.Add(where + " has a level below 1.");
					}

					if (tier.MinLevel < lastLevel) {
						errors.Add(who + " tiers are not sorted by level.");
					}

					lastLevel = tier.MinLevel;
				}
			}

			CheckCounts(errors, "Magic", file.Rarities.Magic, RarityRules.MaxPrefixes(Rarity.Magic) + RarityRules.MaxSuffixes(Rarity.Magic));
			CheckCounts(errors, "Rare", file.Rarities.Rare, RarityRules.MaxPrefixes(Rarity.Rare) + RarityRules.MaxSuffixes(Rarity.Rare));
			return errors;
		}

		private static void CheckCounts(List<string> errors, string rarity, List<AffixCount> counts, int cap)
		{
			foreach (AffixCount count in counts) {
				if (count.Count < 1 || count.Count > cap) {
					errors.Add(rarity + " affix count " + count.Count + " must be between 1 and " + cap + ".");
				}

				if (count.Weight < 0) {
					errors.Add(rarity + " affix count " + count.Count + " has a negative weight.");
				}
			}
		}
	}
}
