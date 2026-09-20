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
		private static readonly HashSet<int> PlaceableTiles = new();

		/// <summary>
		/// Set while the mod edits the world itself, for example when the Hold grows.
		/// Those writes go through the same vanilla placement path a player would use,
		/// so without this the mod would be blocked by its own rules.
		/// </summary>
		public static bool SystemEditInProgress { get; private set; }

		/// <summary>Marks a tile type as breakable despite the global ban.</summary>
		public static void AllowMining(int tileType) => MineableTiles.Add(tileType);

		/// <summary>
		/// Marks a tile type as placeable despite the global ban. The Rift Gate uses
		/// this so a crafted spare is not an item that can never be put down.
		/// </summary>
		public static void AllowPlacement(int tileType) => PlaceableTiles.Add(tileType);

		public static bool IsPlacementAllowed(int tileType)
			=> Unrestricted || PlacementEnabled || PlaceableTiles.Contains(tileType);

		/// <summary>Runs <paramref name="edit"/> with the world-edit rules lifted.</summary>
		public static void AsSystemEdit(System.Action edit)
		{
			bool previous = SystemEditInProgress;
			SystemEditInProgress = true;

			try {
				edit();
			}
			finally {
				SystemEditInProgress = previous;
			}
		}

		/// <summary>True while the rules should not apply at all.</summary>
		private static bool Unrestricted => WorldGen.gen || SystemEditInProgress;

		public static bool CanBreakTile(int tileType)
			=> Unrestricted || MiningEnabled || MineableTiles.Contains(tileType);

		public static bool CanBreakWall(int wallType) => Unrestricted || MiningEnabled;

		public static bool CanPlace() => Unrestricted || PlacementEnabled;

		internal static void Reset()
		{
			MineableTiles.Clear();
			PlaceableTiles.Clear();
			SystemEditInProgress = false;
		}
	}
}
