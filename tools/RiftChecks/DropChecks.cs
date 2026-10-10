using Riftaria.Common.Drops;
using Riftaria.Common.Items;

namespace RiftTools
{
	public sealed class DropReport
	{
		/// <summary>What was checked, tier by tier, with a few sample items.</summary>
		public List<string> Lines { get; } = new();

		public List<string> Failures { get; } = new();

		public bool Ok => Failures.Count == 0;
	}

	/// <summary>
	/// Simulates many rift plans and item rolls and checks every rule the game
	/// relies on. DropCheck runs it on the saved files; the preview page runs it
	/// on unsaved edits through RollServer.
	/// </summary>
	public sealed class DropChecks
	{
		private const int Plans = 2_000;
		private const int Rolls = 100_000;
		private const int Samples = 4;
		private const double Tolerance = 0.005;

		private readonly AffixFile _affixes;
		private readonly LootFile _loot;
		private readonly Func<int, int> _next;
		private readonly DropReport _report = new();

		private DropChecks(AffixFile affixes, LootFile loot, int seed)
		{
			_affixes = affixes;
			_loot = loot;
			_next = new Random(seed).Next;
		}

		public static DropReport Run(AffixFile affixes, LootFile loot, int seed = 12345)
		{
			var checks = new DropChecks(affixes, loot, seed);
			checks.RunAll();
			return checks._report;
		}

		private List<string> Failures => _report.Failures;

		private void Line(string text) => _report.Lines.Add(text);

		private void RunAll()
		{
			foreach (string error in AffixTable.Validate(_affixes)) {
				Failures.Add("affixes.js: " + error);
			}

			foreach (string error in LootTable.Validate(_loot, _affixes)) {
				Failures.Add("loot-pools.js: " + error);
			}

			if (Failures.Count > 0) {
				return;
			}

			foreach (LootTierDef tierDef in _loot.Tiers) {
				int tier = tierDef.Tier;
				var eligible = _loot.Bases.Where(entry => entry.DropsIn(tier)).ToList();
				CheckPlans(tier, tierDef, eligible);

				RiftLootPlan plan = RiftLootPlan.Build(_loot, tier, _next);
				Line($"Tier {tier}: {eligible.Count} bases fit, general pool drew {plan.General.Bases.Count}, boss pool drew {plan.Boss.Bases.Count}");

				CheckRarity(tier, "general", plan.General.Bases, plan.General.Rarity);
				CheckRarity(tier, "boss", plan.Boss.Bases, plan.Boss.Rarity);
				CheckDropChance(tier, plan);
				CheckBoss(tier, plan);
				CheckRolls(tier);
				Line("");
			}
		}

		private void CheckPlans(int tier, LootTierDef def, List<LootBase> eligible)
		{
			int want = def.General.BaseCount <= 0 ? eligible.Count : Math.Min(def.General.BaseCount, eligible.Count);
			for (int i = 0; i < Plans; i++) {
				RiftLootPlan plan = RiftLootPlan.Build(_loot, tier, _next);
				if (plan.General.Bases.Count != want) {
					Failures.Add($"tier {tier}: a plan drew {plan.General.Bases.Count} general bases, expected {want}.");
					return;
				}

				if (plan.General.Bases.Distinct().Count() != plan.General.Bases.Count) {
					Failures.Add($"tier {tier}: a plan drew the same base twice.");
					return;
				}

				if (plan.General.Bases.Concat(plan.Boss.Bases).Any(entry => !entry.DropsIn(tier))) {
					Failures.Add($"tier {tier}: a plan drew a base outside its tier range.");
					return;
				}
			}
		}

		private void CheckRarity(int tier, string pool, List<LootBase> bases, RarityWeights weights)
		{
			int total = weights.Total;
			if (total <= 0 || bases.Count == 0) {
				return;
			}

			var counts = new int[3];
			for (int i = 0; i < Rolls; i++) {
				LootPick pick = RiftLootPlan.Pick(bases, weights, _next);
				counts[(int)pick.Rarity]++;
			}

			int[] expected = { Math.Max(0, weights.Normal), Math.Max(0, weights.Magic), Math.Max(0, weights.Rare) };
			var parts = new List<string>();
			for (int r = 0; r < 3; r++) {
				double actual = (double)counts[r] / Rolls;
				double want = (double)expected[r] / total;
				parts.Add($"{(Rarity)r} {actual:P1}");
				if (Math.Abs(actual - want) > Tolerance) {
					Failures.Add($"tier {tier} {pool}: {(Rarity)r} landed at {actual:P2}, expected {want:P2}.");
				}
			}

			Line($"  {pool,-8} rarity  {string.Join(", ", parts)}");
		}

		private void CheckDropChance(int tier, RiftLootPlan plan)
		{
			int drops = 0;
			for (int i = 0; i < Rolls; i++) {
				if (plan.RollGeneral(_next) != null) {
					drops++;
				}
			}

			double actual = (double)drops / Rolls;
			double want = plan.General.Bases.Count == 0 ? 0 : plan.General.DropChance;
			Line($"  kills    drop    {actual:P2} (expected {want:P2})");
			if (Math.Abs(actual - want) > Tolerance) {
				Failures.Add($"tier {tier}: kills dropped {actual:P2}, expected {want:P2}.");
			}
		}

		private void CheckBoss(int tier, RiftLootPlan plan)
		{
			foreach (string boss in new[] { RiftBossIds.Zombie, RiftBossIds.Eye }) {
				BossPool pool = plan.BossFor(boss);
				BossPool stored = DataJson.Read<BossPool>(DataJson.Write(pool));
				if (stored.Items != pool.Items || stored.Bases.Count != pool.Bases.Count || stored.Rarity.Total != pool.Rarity.Total) {
					Failures.Add($"tier {tier}: the {boss} bag snapshot did not survive a save.");
				}

				List<LootPick> picks = RiftLootPlan.RollBoss(stored, _next);
				if (picks.Count != pool.Items) {
					Failures.Add($"tier {tier}: {boss} bag gave {picks.Count} items, expected {pool.Items}.");
				}

				Line($"  {boss,-10} bag gives {pool.Items}");
			}
		}

		private void CheckRolls(int tier)
		{
			var shortItems = new Dictionary<string, int>();
			var samples = new List<RolledItem>();
			int count = 0;
			foreach (Rarity rarity in new[] { Rarity.Normal, Rarity.Magic, Rarity.Rare }) {
				foreach (LootBase entry in _loot.Bases.Where(b => b.DropsIn(tier))) {
					for (int i = 0; i < Rolls / 100; i++) {
						RolledItem item = ItemRoller.RollItem(_affixes, entry.Item, entry.Tag, tier, rarity, entry.Item, _next);
						count++;
						string problem = Check(item);
						if (problem != null) {
							Failures.Add($"tier {tier}: {item.Name} ({entry.Item}, {rarity}) {problem}");
							return;
						}

						int floor = rarity == Rarity.Rare ? 3 : rarity == Rarity.Magic ? 1 : 0;
						if (item.Affixes.Count < floor) {
							string key = entry.Tag + " " + rarity;
							shortItems[key] = shortItems.GetValueOrDefault(key) + 1;
						}

						if (i == 0 && rarity != Rarity.Normal && samples.Count < Samples && _next(4) == 0) {
							samples.Add(item);
						}
					}
				}
			}

			Line($"  {count} items rolled, every cap, group, tag and tier held");
			foreach (var pair in shortItems) {
				Line($"  note: {pair.Value} {pair.Key} items ran out of affixes that fit");
			}

			foreach (RolledItem item in samples) {
				Line($"  [{item.Rarity}] {item.Name} ({item.Item})");
				foreach (RolledAffix affix in item.Affixes) {
					Line($"      T{affix.Tier} {(affix.Prefix ? "P" : "S")} {AffixTable.Describe(_affixes, affix)}");
				}
			}
		}

		private string Check(RolledItem item)
		{
			int prefixes = item.Affixes.Count(a => a.Prefix);
			int suffixes = item.Affixes.Count - prefixes;
			if (prefixes > RarityRules.MaxPrefixes(item.Rarity) || suffixes > RarityRules.MaxSuffixes(item.Rarity)) {
				return $"has {prefixes} prefixes and {suffixes} suffixes.";
			}

			var groups = new HashSet<string>();
			foreach (RolledAffix rolled in item.Affixes) {
				AffixDef def = AffixTable.FindAffix(_affixes, rolled.Id);
				if (def == null) {
					return $"rolled unknown affix {rolled.Id}.";
				}

				if (!groups.Add(def.GroupKey)) {
					return $"has group {def.GroupKey} twice.";
				}

				if (!def.Tags.Contains(item.Tag)) {
					return $"rolled {def.Id}, which does not fit {item.Tag}.";
				}

				int index = def.Tiers.Count - rolled.Tier;
				if (index < 0 || index >= def.Tiers.Count) {
					return $"rolled {def.Id} at a tier that does not exist.";
				}

				AffixTier tier = def.Tiers[index];
				if (tier.MinLevel > item.Level) {
					return $"rolled {def.Id} T{rolled.Tier}, which needs level {tier.MinLevel}.";
				}

				if (rolled.Value < tier.Min || rolled.Value > tier.Max) {
					return $"rolled {def.Id} at {rolled.Value}, outside {tier.Min}-{tier.Max}.";
				}
			}

			return null;
		}
	}
}
