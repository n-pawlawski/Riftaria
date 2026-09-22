using System;
using System.Collections.Generic;

namespace Riftaria.Common.Progression
{
	/// <summary>Registry of allocatable nodes, in display order.</summary>
	public static class StatTree
	{
		private static readonly List<StatNode> Ordered = new();
		private static readonly Dictionary<string, StatNode> ById = new();

		public static IReadOnlyList<StatNode> Nodes => Ordered;

		public static void Register(StatNode node)
		{
			if (ById.ContainsKey(node.Id)) {
				return;
			}

			ById[node.Id] = node;
			Ordered.Add(node);
		}

		public static StatNode Get(string id) => ById.TryGetValue(id, out StatNode node) ? node : null;

		internal static void LoadDefaults()
		{
			Register(new StatNode {
				Id = "Might",
				Kind = StatKind.DamagePercent,
				PerRank = 0.02f,
				MaxRank = 20,
				RequiredLevel = 1
			});

			Register(new StatNode {
				Id = "Vitality",
				Kind = StatKind.MaxLife,
				PerRank = 5f,
				MaxRank = 20,
				RequiredLevel = 1
			});

			Register(new StatNode {
				Id = "Hardened",
				Kind = StatKind.Defense,
				PerRank = 1f,
				MaxRank = 15,
				RequiredLevel = 1
			});

			Register(new StatNode {
				Id = "Precision",
				Kind = StatKind.CritChance,
				PerRank = 1f,
				MaxRank = 15,
				RequiresNodeId = "Might",
				RequiresRank = 3,
				RequiredLevel = 4
			});

			Register(new StatNode {
				Id = "Fleetfoot",
				Kind = StatKind.MoveSpeed,
				PerRank = 0.02f,
				MaxRank = 10,
				RequiresNodeId = "Vitality",
				RequiresRank = 3,
				RequiredLevel = 4
			});
		}

		/// <summary>Whether a node's level and prerequisite conditions are met.</summary>
		public static bool IsUnlocked(StatNode node, int level, Func<string, int> rankOf)
		{
			if (node == null || level < node.RequiredLevel) {
				return false;
			}

			if (string.IsNullOrEmpty(node.RequiresNodeId)) {
				return true;
			}

			return rankOf(node.RequiresNodeId) >= node.RequiresRank;
		}

		internal static void Reset()
		{
			Ordered.Clear();
			ById.Clear();
		}
	}
}
