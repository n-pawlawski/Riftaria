namespace Riftaria.Common.Maps
{
	/// <summary>
	/// A difficulty step. Tiers describe how a rift is built; they deliberately hold
	/// no NPC or item IDs, because those are only valid after content loads and would
	/// pin the registry to load order.
	///
	/// Access is not stored here. A tier is open when the one below it has been
	/// cleared, which is derived from the Hold rather than declared per tier.
	/// </summary>
	public sealed class RiftTier
	{
		public int Tier { get; init; }

		/// <summary>Rooms carved along the path from spawn to the objective.</summary>
		public int RoomCount { get; init; }

		/// <summary>Multiplier on enemy health and damage inside the rift.</summary>
		public float EnemyScaling { get; init; }

		/// <summary>Concurrent enemies a rift of this tier aims to keep alive.</summary>
		public int EnemyBudget { get; init; }
	}
}
