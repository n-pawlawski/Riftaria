using System.Collections.Generic;
using Terraria;

namespace Rifthold.Common.World
{
	/// <summary>
	/// The single authority on whether the world may be reshaped. Every mining,
	/// explosion and placement hook defers to this, so opening up a resource
	/// node later means adding an allowlist entry rather than editing hooks.
	/// </summary>
	public static class WorldEditPolicy
	{
		/// <summary>Global switch for breaking blocks. The MVP ships with this off.</summary>
		public const bool MiningEnabled = false;

		/// <summary>
		/// Global switch for placing blocks and walls. Off for the same reason mining
		/// is: players pillaring over an encounter or sealing a corridor would bypass
		/// the authored layout entirely.
		/// </summary>
		public const bool PlacementEnabled = false;

		private static readonly HashSet<int> MineableTiles = new();

		/// <summary>Marks a tile type as breakable despite the global ban.</summary>
		public static void AllowMining(int tileType) => MineableTiles.Add(tileType);

		public static bool CanBreakTile(int tileType)
		{
			// World generation writes and clears tiles constantly. Blocking it there
			// would break our own gen passes as well as vanilla's.
			if (WorldGen.gen) {
				return true;
			}

			return MiningEnabled || MineableTiles.Contains(tileType);
		}

		public static bool CanBreakWall(int wallType)
		{
			if (WorldGen.gen) {
				return true;
			}

			return MiningEnabled;
		}

		public static bool CanPlace()
		{
			if (WorldGen.gen) {
				return true;
			}

			return PlacementEnabled;
		}

		internal static void Reset() => MineableTiles.Clear();
	}
}
