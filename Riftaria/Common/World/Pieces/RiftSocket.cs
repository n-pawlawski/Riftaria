namespace Riftaria.Common.World.Pieces
{
	/// <summary>What a socket may be used for when pieces are joined.</summary>
	public enum SocketRole
	{
		/// <summary>Entry, exit or branch.</summary>
		Any,

		/// <summary>Only the way in. The start piece has none.</summary>
		Entry,

		/// <summary>The way on, toward the boss, or into a branch.</summary>
		Exit,

		/// <summary>Only into a side branch.</summary>
		Branch
	}

	/// <summary>
	/// An opening on a piece. X and Y are the middle of the opening in the
	/// piece's tile space, measured on tile edges. Angle points out of the piece
	/// in degrees, with Y down, so 0 is right, 90 is down and -90 is up.
	/// </summary>
	public readonly struct RiftSocket
	{
		public double X { get; }

		public double Y { get; }

		public double Angle { get; }

		/// <summary>Size of the opening across its direction, in tiles.</summary>
		public int Width { get; }

		public SocketRole Role { get; }

		public RiftSocket(double x, double y, double angle, int width, SocketRole role = SocketRole.Any)
		{
			X = x;
			Y = y;
			Angle = angle;
			Width = width;
			Role = role;
		}

		public bool CanEnter => Role == SocketRole.Any || Role == SocketRole.Entry;

		public bool CanLeave => Role == SocketRole.Any || Role == SocketRole.Exit;

		public bool CanBranch => Role != SocketRole.Entry;
	}
}
