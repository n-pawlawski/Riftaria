using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	[Flags]
	public enum PieceTag
	{
		None = 0,
		Start = 1,
		Room = 2,
		Boss = 4,
		DeadEnd = 8
	}

	/// <summary>
	/// Draws a piece with its top-left corner at (ox, oy). <paramref name="used"/>
	/// lines up with the piece's sockets, so a piece can add the steps that
	/// reach an opening only when that opening leads somewhere.
	/// </summary>
	public delegate void PieceStamp(RiftGrid grid, int ox, int oy, bool[] used);

	/// <summary>
	/// One shape the assembler can place. Its tile space runs from (0, 0) to
	/// (<see cref="Width"/>, <see cref="Height"/>). Pieces never rotate, because a
	/// turned room would no longer have a floor to walk on.
	/// </summary>
	public sealed class RiftPiece
	{
		public string Name { get; init; }

		public PieceTag Tags { get; init; }

		/// <summary>Carved area. The floor under the walk is rock below this box.</summary>
		public int Width { get; init; }

		public int Height { get; init; }

		public IReadOnlyList<RiftSocket> Sockets { get; init; } = Array.Empty<RiftSocket>();

		public PieceStamp Stamp { get; init; }

		/// <summary>Enemies, in the piece's tile space. Y is the row under their feet.</summary>
		public IReadOnlyList<RiftMark> Marks { get; init; } = Array.Empty<RiftMark>();

		/// <summary>Solid rock kept between this piece and anything it is not joined to.</summary>
		public int Margin { get; init; } = RiftAssembler.Margin;

		/// <summary>Feet of the player on entry. Only the start piece needs one.</summary>
		public int SpawnX { get; init; }

		public int SpawnY { get; init; }

		/// <summary>Set on the boss piece.</summary>
		public RiftArena Arena { get; init; }

		private const int BandRows = 6;

		private List<RiftBox> _footprint;

		/// <summary>
		/// The carved cells as a stack of short bands, in the piece's tile space.
		/// Closer to a round or slanted room than its whole box, so an angled
		/// socket on a sloped face is not buried inside the piece's own area.
		/// </summary>
		public IReadOnlyList<RiftBox> Footprint()
		{
			if (_footprint != null) {
				return _footprint;
			}

			var grid = new RiftGrid(Width, Height, 0);
			Stamp(grid, 0, 0, new bool[Sockets.Count]);
			_footprint = new List<RiftBox>();
			for (int top = 0; top < Height; top += BandRows) {
				int bottom = Math.Min(Height, top + BandRows);
				int left = int.MaxValue;
				int right = int.MinValue;
				for (int y = top; y < bottom; y++) {
					for (int x = 0; x < Width; x++) {
						if (grid.Get(x, y) == RiftGrid.Rock) {
							continue;
						}

						left = Math.Min(left, x);
						right = Math.Max(right, x);
					}
				}

				if (left <= right) {
					_footprint.Add(new RiftBox(left, top, right + 1, bottom));
				}
			}

			return _footprint;
		}
	}

	/// <summary>An enemy placed on the path. Kind is up to the tier's catalog.</summary>
	public readonly struct RiftMark
	{
		public int X { get; }

		public int Y { get; }

		public int Kind { get; }

		public RiftMark(int x, int y, int kind)
		{
			X = x;
			Y = y;
			Kind = kind;
		}
	}

	/// <summary>The boss chamber inside the boss piece, in the piece's tile space.</summary>
	public sealed class RiftArena
	{
		public int Left { get; init; }

		public int Top { get; init; }

		public int Right { get; init; }

		public int Bottom { get; init; }

		public int ArenaX { get; init; }

		public int ArenaY { get; init; }

		/// <summary>Where the boss stands. Negative when it spawns at the arena point instead.</summary>
		public int FeetX { get; init; }

		public int FeetY { get; init; }

		public int PortalX { get; init; }

		public int PortalY { get; init; }
	}
}
