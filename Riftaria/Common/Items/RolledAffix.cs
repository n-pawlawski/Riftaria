using System.Collections.Generic;

namespace Riftaria.Common.Items
{
	/// <summary>
	/// One affix on an item. The stat and kind are copied from the definition at
	/// roll time, so an item keeps working after its affix is edited or removed.
	/// Tier 1 is the best tier of that affix.
	/// </summary>
	public sealed class RolledAffix
	{
		public string Id { get; set; } = "";

		public bool Prefix { get; set; }

		public string Stat { get; set; } = "";

		public int Tier { get; set; }

		public int Value { get; set; }
	}

	/// <summary>A finished roll: the base, its rarity, its affixes and the name shown in game.</summary>
	public sealed class RolledItem
	{
		public string Item { get; set; } = "";

		public string Tag { get; set; } = "";

		public int Level { get; set; }

		public Rarity Rarity { get; set; }

		public string Name { get; set; } = "";

		public List<RolledAffix> Affixes { get; set; } = new();
	}
}
