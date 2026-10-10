namespace Riftaria.Common.World.Pieces
{
	/// <summary>A piece at its place in the plan, before the plan is moved onto the world grid.</summary>
	public sealed class PlacedPiece
	{
		public RiftPiece Piece { get; }

		public int X { get; }

		public int Y { get; }

		/// <summary>Owner number for the overlap test. Links use the same numbering.</summary>
		public int Id { get; }

		/// <summary>Which sockets are joined to something.</summary>
		public bool[] Used { get; }

		public PlacedPiece(RiftPiece piece, int x, int y, int id)
		{
			Piece = piece;
			X = x;
			Y = y;
			Id = id;
			Used = new bool[piece.Sockets.Count];
		}

		public RiftBox Box => new(X, Y, X + Piece.Width, Y + Piece.Height);

		public RiftSocket Socket(int index) => Piece.Sockets[index];

		public double SocketX(int index) => X + Piece.Sockets[index].X;

		public double SocketY(int index) => Y + Piece.Sockets[index].Y;
	}
}
