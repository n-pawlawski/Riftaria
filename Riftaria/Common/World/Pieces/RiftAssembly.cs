using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>An assembled rift, already on its world grid. Every position is in world tiles.</summary>
	public sealed class RiftAssembly
	{
		public RiftGrid Grid { get; init; }

		public int SpawnX { get; init; }

		public int SpawnY { get; init; }

		public int RoomLeft { get; init; }

		public int RoomTop { get; init; }

		public int RoomRight { get; init; }

		public int RoomBottom { get; init; }

		public int ArenaX { get; init; }

		public int ArenaY { get; init; }

		public int FeetX { get; init; }

		public int FeetY { get; init; }

		public int PortalX { get; init; }

		public int PortalY { get; init; }

		public List<RiftMark> Marks { get; init; }

		/// <summary>Pieces in plan space. Add <see cref="OffsetX"/> and <see cref="OffsetY"/> for world tiles.</summary>
		public IReadOnlyList<PlacedPiece> Pieces { get; init; }

		/// <summary>Connectors in plan space, offset the same way as <see cref="Pieces"/>.</summary>
		public IReadOnlyList<RiftConnector> Connectors { get; init; }

		public int OffsetX { get; init; }

		public int OffsetY { get; init; }

		public int MainRooms { get; init; }

		public int Branches { get; init; }
	}
}
