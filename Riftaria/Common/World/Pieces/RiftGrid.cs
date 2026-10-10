using System;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// The rift as a tile grid, in the cell values the stamps and the preview
	/// read. Nothing is drawn inside <see cref="Edge"/> of the sides.
	/// </summary>
	public sealed class RiftGrid
	{
		public const byte Rock = 0;
		public const byte Air = 1;
		public const byte Boss = 2;
		public const byte Floor = 3;
		public const byte Ledge = 4;
		public const byte Platform = 5;

		public int Width { get; }

		public int Height { get; }

		public int Edge { get; }

		public byte[] Cells { get; }

		public RiftGrid(int width, int height, int edge)
		{
			Width = width;
			Height = height;
			Edge = edge;
			Cells = new byte[width * height];
		}

		public bool Inside(int x, int y)
			=> x >= Edge && y >= Edge && x < Width - Edge && y < Height - Edge;

		public byte Get(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height) {
				return Rock;
			}

			return Cells[y * Width + x];
		}

		public void Set(int x, int y, byte value)
		{
			if (Inside(x, y)) {
				Cells[y * Width + x] = value;
			}
		}

		public bool IsOpen(int x, int y)
		{
			byte cell = Get(x, y);
			return cell == Air || cell == Boss || cell == Floor;
		}

		public void Fill(int left, int top, int width, int height, byte value)
		{
			for (int y = top; y < top + height; y++) {
				for (int x = left; x < left + width; x++) {
					Set(x, y, value);
				}
			}
		}

		/// <summary>Top half of an ellipse standing on <paramref name="floorY"/>.</summary>
		public void Dome(int left, int floorY, int width, int rise, byte value)
		{
			double rx = width / 2.0;
			double cx = left + rx;
			for (int y = floorY - rise; y < floorY; y++) {
				for (int x = left; x < left + width; x++) {
					double dx = (x + 0.5 - cx) / rx;
					double dy = (y + 0.5 - floorY) / rise;
					if (dx * dx + dy * dy <= 1.0) {
						Set(x, y, value);
					}
				}
			}
		}

		public void Diamond(int left, int top, int width, int height, byte value)
		{
			double rx = width / 2.0;
			double ry = height / 2.0;
			double cx = left + rx;
			double cy = top + ry;
			for (int y = top; y < top + height; y++) {
				for (int x = left; x < left + width; x++) {
					double dx = Math.Abs(x + 0.5 - cx) / rx;
					double dy = Math.Abs(y + 0.5 - cy) / ry;
					if (dx + dy <= 1.0) {
						Set(x, y, value);
					}
				}
			}
		}

		/// <summary>Cells whose centre lies inside the circle. Rows below <paramref name="maxY"/> are skipped.</summary>
		public void Disk(double cx, double cy, double radius, byte value, bool rockOnly, int maxY = int.MaxValue)
		{
			int x0 = (int)Math.Floor(cx - radius) - 1;
			int y0 = (int)Math.Floor(cy - radius) - 1;
			int x1 = (int)Math.Ceiling(cx + radius) + 1;
			int y1 = Math.Min((int)Math.Ceiling(cy + radius) + 1, maxY - 1);
			double limit = radius * radius;
			for (int y = y0; y <= y1; y++) {
				for (int x = x0; x <= x1; x++) {
					double dx = x + 0.5 - cx;
					double dy = y + 0.5 - cy;
					if (dx * dx + dy * dy > limit) {
						continue;
					}

					if (rockOnly && Get(x, y) != Rock) {
						continue;
					}

					Set(x, y, value);
				}
			}
		}

		/// <summary>A straight band from one point to another, <paramref name="thick"/> tiles across.</summary>
		public void Beam(double x0, double y0, double x1, double y1, double thick, byte value, bool rockOnly)
		{
			double length = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
			if (length < 0.001) {
				return;
			}

			double c = (x1 - x0) / length;
			double s = (y1 - y0) / length;
			double halfT = thick / 2.0;
			int bx0 = (int)Math.Floor(Math.Min(x0, x1) - halfT) - 1;
			int by0 = (int)Math.Floor(Math.Min(y0, y1) - halfT) - 1;
			int bx1 = (int)Math.Ceiling(Math.Max(x0, x1) + halfT) + 1;
			int by1 = (int)Math.Ceiling(Math.Max(y0, y1) + halfT) + 1;
			for (int y = by0; y <= by1; y++) {
				for (int x = bx0; x <= bx1; x++) {
					double dx = x + 0.5 - x0;
					double dy = y + 0.5 - y0;
					double along = dx * c + dy * s;
					double across = -dx * s + dy * c;
					if (along < 0 || along > length || Math.Abs(across) > halfT) {
						continue;
					}

					if (rockOnly && Get(x, y) != Rock) {
						continue;
					}

					Set(x, y, value);
				}
			}
		}

		/// <summary>A row of ledges or platforms. Only open cells change, so walls stay whole.</summary>
		public void Line(int x0, int x1, int y, byte value)
		{
			for (int x = x0; x < x1; x++) {
				byte cell = Get(x, y);
				if (cell == Air || cell == Boss) {
					Set(x, y, value);
				}
			}
		}

		/// <summary>
		/// Platform strips a rung apart from the floor up to an opening, drifting
		/// from <paramref name="fromX"/> toward <paramref name="toX"/>. The last strip
		/// is close enough to jump into the opening.
		/// </summary>
		public void StairsTo(int fromX, int floorY, int toX, int openingBottom)
		{
			int pitch = RiftConnector.RungPitch;
			int rise = floorY - openingBottom;
			if (rise <= pitch) {
				return;
			}

			for (int y = floorY - pitch; y >= openingBottom; y -= pitch) {
				double t = (floorY - y) / (double)rise;
				int x = (int)Math.Round(fromX + (toX - fromX) * t);
				Line(x - 3, x + 4, y, Platform);
			}
		}

		/// <summary>Open cells standing on rock become floor.</summary>
		public void LayFloors()
		{
			for (int y = Edge; y < Height - Edge - 1; y++) {
				for (int x = Edge; x < Width - Edge; x++) {
					int here = y * Width + x;
					byte below = Cells[(y + 1) * Width + x];
					if ((Cells[here] == Air || Cells[here] == Boss) && below == Rock) {
						Cells[here] = Floor;
					}
				}
			}
		}
	}
}
