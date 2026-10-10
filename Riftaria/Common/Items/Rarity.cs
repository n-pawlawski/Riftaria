namespace Riftaria.Common.Items
{
	public enum Rarity
	{
		Normal,
		Magic,
		Rare
	}

	/// <summary>The prefix and suffix caps are fixed. How many affixes a roll aims for comes from affixes.js.</summary>
	public static class RarityRules
	{
		public static int MaxPrefixes(Rarity rarity) => rarity switch {
			Rarity.Magic => 1,
			Rarity.Rare => 3,
			_ => 0
		};

		public static int MaxSuffixes(Rarity rarity) => MaxPrefixes(rarity);
	}
}
