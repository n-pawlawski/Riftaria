namespace Riftaria.Common.World.Pieces
{
	/// <summary>Axis-aligned area in tile space, used to keep pieces apart.</summary>
	public readonly struct RiftBox
	{
		public double MinX { get; }

		public double MinY { get; }

		public double MaxX { get; }

		public double MaxY { get; }

		public RiftBox(double minX, double minY, double maxX, double maxY)
		{
			MinX = minX;
			MinY = minY;
			MaxX = maxX;
			MaxY = maxY;
		}

		public RiftBox Inflate(double by) => new(MinX - by, MinY - by, MaxX + by, MaxY + by);

		public bool Intersects(RiftBox other)
			=> MinX < other.MaxX && other.MinX < MaxX && MinY < other.MaxY && other.MinY < MaxY;
	}
}
