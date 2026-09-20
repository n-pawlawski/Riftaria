namespace Rifthold.Common.Maps
{
	/// <summary>
	/// A difficulty step. Tiers describe how a rift is built and what it pays out;
	/// they deliberately hold no NPC or item IDs, because those are only valid
	/// after content loads and would pin the registry to load order.
	/// </summary>
	public sealed class RiftTier
	{
		public int Tier { get; init; }

		/// <summary>Character level required before the Map Device will open this rift.</summary>
		public int RequiredLevel { get; init; }

		/// <summary>Rooms carved along the path from spawn to the objective.</summary>
		public int RoomCount { get; init; }

		/// <summary>Multiplier on enemy health and damage inside the rift.</summary>
		public float EnemyScaling { get; init; }

		/// <summary>Concurrent enemies the spawner aims to keep alive.</summary>
		public int EnemyBudget { get; init; }

		/// <summary>Experience for clearing the rift, before per-kill experience.</summary>
		public int CompletionXp { get; init; }

		/// <summary>Essence paid out on completion.</summary>
		public int EssenceReward { get; init; }

		/// <summary>Chance in [0,1] that clearing drops a sigil for the next tier up.</summary>
		public float NextTierSigilChance { get; init; }
	}
}
