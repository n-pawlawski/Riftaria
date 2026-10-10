using System.Text.Json;
using Riftaria.Common.Drops;
using Riftaria.Common.Items;
using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;
using RiftTools;

namespace RollServer
{
	/// <summary>
	/// The requests the preview page can make. Each one carries the page's
	/// current affixes and loot, saved or not, so previews reflect unsaved edits.
	/// </summary>
	internal static class Rolls
	{
		private const int MaxTrials = 1_000_000;

		private static readonly Rarity[] AllRarities = { Rarity.Normal, Rarity.Magic, Rarity.Rare };

		public static object Handle(JsonElement request, Func<int, int> next)
		{
			string op = Text(request, "op");
			return op switch {
				"meta" => Meta(),
				"layout" => Layout(request),
				"inspect" => Inspect(Affixes(request), Loot(request)),
				"samples" => Samples(request, next),
				"stats" => Stats(request, next),
				"names" => Names(Affixes(request), Math.Clamp(Int(request, "count", 8), 1, 100), next),
				"check" => DropChecks.Run(Affixes(request), Loot(request)),
				_ => throw new InvalidOperationException("Unknown request " + op + ".")
			};
		}

		/// <summary>Facts that live in code: the stat ids the game knows and the affix caps.</summary>
		private static object Meta() => new {
			stats = Enum.GetNames<StatId>(),
			caps = new {
				magic = RarityRules.MaxPrefixes(Rarity.Magic),
				rare = RarityRules.MaxPrefixes(Rarity.Rare)
			}
		};

		/// <summary>The tile grid the game carves for this tier. Matches the biomes in the tier table.</summary>
		private static object Layout(JsonElement request)
		{
			RiftPiecePlan plan = Int(request, "tier", 1) switch {
				2 => RiftPlans.Evil,
				3 => RiftPlans.Stone,
				_ => RiftPlans.Forest
			};
			plan.Roll();
			return Grid(plan.Width, plan.Height, plan.Assembly.Grid.Cells);
		}

		private static object Grid(int width, int height, byte[] cells) => new {
			width,
			height,
			shell = RiftMeasures.Border,
			cells
		};

		/// <summary>
		/// Everything the editor shows that comes from game rules: the problems
		/// that block a save, which bases fit each tier, and each affix's roll
		/// weight by level. Lists line up with the order in the files.
		/// </summary>
		private static object Inspect(AffixFile affixes, LootFile loot) => new {
			affixErrors = AffixTable.Validate(affixes),
			lootErrors = LootTable.Validate(loot, affixes),
			affixProblems = AffixProblems(affixes),
			tiers = loot.Tiers.Select(tier => new {
				tier = tier.Tier,
				bases = Enumerable.Range(0, loot.Bases.Count).Where(i => loot.Bases[i].DropsIn(tier.Tier)).ToArray()
			}),
			affixWeights = affixes.Affixes.Select(affix => affix.Tiers
				.Select(tier => tier.MinLevel)
				.Distinct()
				.OrderBy(level => level)
				.Select(level => new { level, weight = ItemRoller.UnlockedWeight(affix, level) }))
		};

		/// <summary>
		/// The problems that belong to each affix, by position. Each affix is
		/// checked alone so two affixes sharing an id cannot swap problems.
		/// </summary>
		private static List<List<string>> AffixProblems(AffixFile affixes)
		{
			var shared = new HashSet<string>(AffixTable.Validate(With(affixes, new List<AffixDef>())));
			var result = new List<List<string>>();
			foreach (AffixDef affix in affixes.Affixes) {
				var problems = AffixTable.Validate(With(affixes, new List<AffixDef> { affix }))
					.Where(message => !shared.Contains(message))
					.ToList();
				if (!string.IsNullOrEmpty(affix.Id) && affixes.Affixes.Count(other => other.Id == affix.Id) > 1) {
					problems.Insert(0, affix.Id + " is listed twice.");
				}

				result.Add(problems);
			}

			return result;
		}

		private static AffixFile With(AffixFile file, List<AffixDef> affixes) => new() {
			Tags = file.Tags,
			Stats = file.Stats,
			Rarities = file.Rarities,
			RareNames = file.RareNames,
			Affixes = affixes
		};

		private static object Samples(JsonElement request, Func<int, int> next)
		{
			AffixFile affixes = Affixes(request);
			LootFile loot = Loot(request);
			int tier = Int(request, "tier", 1);
			int count = Math.Clamp(Int(request, "count", 12), 1, 200);
			string source = Text(request, "source");
			Rarity? forced = Forced(Text(request, "rarity"));
			var items = new List<object>();
			string note = "";

			if (source == "base") {
				LootBase entry = BaseAt(loot, request);
				for (int i = 0; i < count; i++) {
					items.Add(View(affixes, Roll(affixes, entry, tier, forced ?? Rarity.Magic, next)));
				}

				return new { note, items };
			}

			RiftLootPlan plan = RiftLootPlan.Build(loot, tier, next);
			var picks = new List<LootPick>();
			if (source == "boss") {
				BossPool pool = plan.BossFor(Text(request, "boss"));
				note = $"This rift's boss bag gives {pool.Items} items from: {Names(pool.Bases)}.";
				while (picks.Count < count && pool.Items > 0 && pool.Bases.Count > 0) {
					picks.AddRange(RiftLootPlan.RollBoss(pool, next));
				}
			}
			else {
				note = $"This rift's kills drop {plan.General.DropChance * 100:0.0}% of the time, from: {Names(plan.General.Bases)}.";
				for (int i = 0; i < count; i++) {
					LootPick pick = RiftLootPlan.Pick(plan.General.Bases, plan.General.Rarity, next);
					if (pick != null) {
						picks.Add(pick);
					}
				}
			}

			foreach (LootPick pick in picks.Take(count)) {
				items.Add(View(affixes, Roll(affixes, pick.Base, tier, forced ?? pick.Rarity, next)));
			}

			if (picks.Count == 0) {
				note += " Nothing to roll.";
			}

			return new { note, items };
		}

		/// <summary>
		/// Many rolls from one source. Pool sources build a fresh rift plan for
		/// every item, so the numbers cover many rifts rather than one.
		/// </summary>
		private static object Stats(JsonElement request, Func<int, int> next)
		{
			AffixFile affixes = Affixes(request);
			LootFile loot = Loot(request);
			int tier = Int(request, "tier", 1);
			int trials = Math.Clamp(Int(request, "trials", 10_000), 1, MaxTrials);
			string source = Text(request, "source");
			string boss = Text(request, "boss");
			Rarity? forced = Forced(Text(request, "rarity"));

			Func<LootPick> picker;
			if (source == "base") {
				LootBase entry = BaseAt(loot, request);
				picker = () => new LootPick { Base = entry, Rarity = forced ?? Rarity.Magic };
			}
			else {
				picker = () => {
					RiftLootPlan plan = RiftLootPlan.Build(loot, tier, next);
					LootPick pick = source == "boss"
						? RiftLootPlan.Pick(plan.BossFor(boss).Bases, plan.BossFor(boss).Rarity, next)
						: RiftLootPlan.Pick(plan.General.Bases, plan.General.Rarity, next);
					return pick == null || forced == null ? pick : new LootPick { Base = pick.Base, Rarity = forced.Value };
				};
			}

			var rarityCounts = new int[3];
			var prefixes = new int[3];
			var suffixes = new int[3];
			var bases = new Dictionary<string, int>();
			var tally = new Dictionary<string, AffixTally>();
			int rolled = 0;
			int affixed = 0;
			int rareShort = 0;

			for (int i = 0; i < trials; i++) {
				LootPick pick = picker();
				if (pick == null) {
					continue;
				}

				rolled++;
				RolledItem item = Roll(affixes, pick.Base, tier, pick.Rarity, next);
				int r = (int)item.Rarity;
				rarityCounts[r]++;
				bases[item.Item] = bases.GetValueOrDefault(item.Item) + 1;
				if (item.Rarity == Rarity.Normal) {
					continue;
				}

				affixed++;
				if (item.Rarity == Rarity.Rare && item.Affixes.Count < 3) {
					rareShort++;
				}

				foreach (RolledAffix affix in item.Affixes) {
					if (affix.Prefix) {
						prefixes[r]++;
					}
					else {
						suffixes[r]++;
					}

					if (!tally.TryGetValue(affix.Id, out AffixTally entry)) {
						entry = new AffixTally { Prefix = affix.Prefix };
						tally[affix.Id] = entry;
					}

					entry.Count++;
					entry.Total += affix.Value;
					entry.Tiers[affix.Tier] = entry.Tiers.GetValueOrDefault(affix.Tier) + 1;
				}
			}

			return new {
				rolled,
				affixed,
				rareShort,
				rarity = AllRarities.Select(rarity => new {
					rarity,
					count = rarityCounts[(int)rarity],
					prefixes = prefixes[(int)rarity],
					suffixes = suffixes[(int)rarity]
				}),
				bases = bases.OrderByDescending(pair => pair.Value).Select(pair => new { item = pair.Key, count = pair.Value }),
				affixes = tally.OrderByDescending(pair => pair.Value.Count).Select(pair => new {
					id = pair.Key,
					prefix = pair.Value.Prefix,
					count = pair.Value.Count,
					total = pair.Value.Total,
					tiers = pair.Value.Tiers.OrderBy(t => t.Key).Select(t => new { tier = t.Key, count = t.Value })
				})
			};
		}

		private static List<string> Names(AffixFile affixes, int count, Func<int, int> next)
		{
			var names = new List<string>(count);
			for (int i = 0; i < count; i++) {
				names.Add(ItemRoller.Compose(affixes, Rarity.Rare, Array.Empty<RolledAffix>(), "(base)", next));
			}

			return names;
		}

		/// <summary>The preview shows the ItemID name; the game shows the localized one.</summary>
		private static RolledItem Roll(AffixFile affixes, LootBase entry, int tier, Rarity rarity, Func<int, int> next)
			=> ItemRoller.RollItem(affixes, entry.Item, entry.Tag, tier, rarity, entry.Item, next);

		private static object View(AffixFile affixes, RolledItem item) => new {
			name = item.Name,
			item = item.Item,
			tag = item.Tag,
			level = item.Level,
			rarity = item.Rarity,
			affixes = item.Affixes.Select(affix => new {
				id = affix.Id,
				prefix = affix.Prefix,
				tier = affix.Tier,
				value = affix.Value,
				text = AffixTable.Describe(affixes, affix)
			})
		};

		private static string Names(List<LootBase> bases) => bases.Count == 0 ? "nothing" : string.Join(", ", bases.Select(entry => entry.Item));

		private static LootBase BaseAt(LootFile loot, JsonElement request)
		{
			int index = Int(request, "baseIndex", -1);
			if (index < 0 || index >= loot.Bases.Count) {
				throw new InvalidOperationException("That base does not exist.");
			}

			return loot.Bases[index];
		}

		private static Rarity? Forced(string rarity) => rarity switch {
			"normal" => Rarity.Normal,
			"magic" => Rarity.Magic,
			"rare" => Rarity.Rare,
			_ => null
		};

		private static AffixFile Affixes(JsonElement request) => AffixTable.Parse(request.GetProperty("affixes").GetRawText());

		private static LootFile Loot(JsonElement request) => LootTable.Parse(request.GetProperty("loot").GetRawText());

		private static string Text(JsonElement request, string name)
			=> request.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : "";

		private static int Int(JsonElement request, string name, int fallback)
			=> request.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Number ? value.GetInt32() : fallback;

		private sealed class AffixTally
		{
			public bool Prefix;

			public int Count;

			public long Total;

			public readonly Dictionary<int, int> Tiers = new();
		}
	}
}
