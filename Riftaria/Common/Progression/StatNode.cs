namespace Riftaria.Common.Progression
{
	public enum StatKind
	{
		DamagePercent,
		MaxLife,
		Defense,
		CritChance,
		MoveSpeed
	}

	/// <summary>
	/// One allocatable node. The MVP presents these as a flat list of stats, but
	/// they already carry prerequisites and level gates, so growing this into a
	/// real tree is a data and UI change rather than a rewrite of progression.
	/// </summary>
	public sealed class StatNode
	{
		public string Id { get; init; }

		public StatKind Kind { get; init; }

		/// <summary>Value added per rank. Percent kinds are fractions, so 0.02 is 2%.</summary>
		public float PerRank { get; init; }

		public int MaxRank { get; init; }

		/// <summary>Node that must be invested in first, or null when always available.</summary>
		public string RequiresNodeId { get; init; }

		/// <summary>Ranks required in <see cref="RequiresNodeId"/> before this unlocks.</summary>
		public int RequiresRank { get; init; }

		/// <summary>Character level required before this unlocks.</summary>
		public int RequiredLevel { get; init; }
	}
}
