namespace Riftaria.Common.World
{
	/// <summary>
	/// Numbers the layout and the preview both read. Keeping them here means a
	/// rift on the page is the one the game carves.
	/// </summary>
	public static class RiftMeasures
	{
		/// <summary>Tiles of solid ground around a boss chamber, apart from the entrance.</summary>
		public const int Buffer = 50;

		/// <summary>Sealed band around the edge of a rift.</summary>
		public const int Border = 24;

		public const int CircleHeight = 350;

		public const int PassageHeight = 7;

		/// <summary>How far a forest floor may drift, in tiles. Matches the forest profile.</summary>
		public const int ForestRange = 10;

		public static int CircleFloorY => CircleHeight / 2;

		public static int SpawnX => Border + 40;
	}
}
