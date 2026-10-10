namespace Riftaria.Common.World
{
	/// <summary>
	/// One carved chamber. <see cref="Floors"/> is the shared heightmap for the
	/// whole rift, indexed by tile X, so a feature can read the ground under any column.
	/// </summary>
	public sealed class BiomeRoom
	{
		public int Left { get; init; }

		public int Right { get; init; }

		/// <summary>Air height at the middle of the room, above the local floor.</summary>
		public int Height { get; init; }

		/// <summary>
		/// Highest air row in the room. A round room's ceiling is lower than this
		/// toward the mouths; use <see cref="CeilingAt"/> for a column.
		/// </summary>
		public int Ceiling { get; init; }

		public int[] Floors { get; init; }

		/// <summary>Circle stamped on the path. The floor stays the walkable rectangle.</summary>
		public bool Round { get; init; }

		/// <summary>Horizontal radius of a circle room, in tiles.</summary>
		public int Radius { get; init; }

		/// <summary>
		/// Tile row of the circle's centre. This sits a few tiles above the floor,
		/// so the carved part is the upper body of the circle and the rectangle.
		/// </summary>
		public int CircleCenterY { get; init; }

		/// <summary>Arena rooms stay bare so dressing does not crowd the fight.</summary>
		public bool Dress { get; init; }

		public int Center => Left + ((Right - Left) / 2);

		/// <summary>
		/// First air row of this circle in the column. Outside the radius the
		/// circle does not raise the ceiling; the rectangle still opens the path.
		/// </summary>
		public int CeilingAt(int x)
		{
			if (!Round || Radius <= 0) {
				return Ceiling;
			}

			return RiftCircleFloors.Ceiling(x, Center, Radius, CircleCenterY);
		}

		public int FloorAt(int x)
		{
			if (Floors == null || x < 0 || x >= Floors.Length) {
				return RiftMeasures.CircleFloorY;
			}

			return Floors[x];
		}
	}
}
