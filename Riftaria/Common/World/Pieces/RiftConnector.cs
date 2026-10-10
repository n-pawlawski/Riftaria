using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>One straight run of a connector, in plan tile space.</summary>
	public readonly struct RiftSegment
	{
		public double X0 { get; }

		public double Y0 { get; }

		public double X1 { get; }

		public double Y1 { get; }

		public double Thick { get; }

		public RiftSegment(double x0, double y0, double x1, double y1, double thick)
		{
			X0 = x0;
			Y0 = y0;
			X1 = x1;
			Y1 = y1;
			Thick = thick;
		}

		public double Length => Math.Sqrt((X1 - X0) * (X1 - X0) + (Y1 - Y0) * (Y1 - Y0));

		/// <summary>Degrees from level, from 0 to 90, whichever way the run goes.</summary>
		public double Grade => Math.Atan2(Math.Abs(Y1 - Y0), Math.Abs(X1 - X0)) * 180.0 / Math.PI;

		/// <summary>Too steep to walk. Mining is off, so the run gets platform rungs.</summary>
		public bool Steep => Grade > RiftConnector.MaxGrade;
	}

	/// <summary>
	/// A tunnel built for one join: a run along the open socket's angle, then an
	/// optional bend into the next piece's entry. Rooms keep their orientation,
	/// so this is what lets a socket face any angle.
	/// </summary>
	public sealed class RiftConnector
	{
		/// <summary>Steepest run the player can walk. Matches the evil rift's steepest room.</summary>
		public const double MaxGrade = 38;

		/// <summary>Rows between rungs, steps and shelves anywhere the path climbs. A plain jump clears this.</summary>
		public const int RungPitch = 5;

		public IReadOnlyList<RiftSegment> Segments { get; }

		private RiftConnector(List<RiftSegment> segments)
		{
			Segments = segments;
		}

		/// <summary>Straight runs through each point in turn. Points closer than a tile are merged.</summary>
		public static RiftConnector Through(double thick, params (double X, double Y)[] points)
		{
			var segments = new List<RiftSegment>();
			(double X, double Y) last = points[0];
			for (int i = 1; i < points.Length; i++) {
				(double X, double Y) next = points[i];
				var segment = new RiftSegment(last.X, last.Y, next.X, next.Y, thick);
				if (segment.Length < 1.0) {
					continue;
				}

				segments.Add(segment);
				last = next;
			}

			return new RiftConnector(segments);
		}

		/// <summary>
		/// Squares along each run for the overlap test, half a tunnel width apart
		/// and a little wider than the tunnel, so a diagonal run is fully covered.
		/// </summary>
		public IEnumerable<RiftBox> Boxes()
		{
			foreach (RiftSegment segment in Segments) {
				double length = segment.Length;
				double half = segment.Thick / 2.0 * 1.1;
				int steps = Math.Max(1, (int)Math.Ceiling(length / (segment.Thick / 2.0)));
				for (int i = 0; i <= steps; i++) {
					double t = i / (double)steps;
					double x = segment.X0 + (segment.X1 - segment.X0) * t;
					double y = segment.Y0 + (segment.Y1 - segment.Y0) * t;
					yield return new RiftBox(x - half, y - half, x + half, y + half);
				}
			}
		}

		/// <summary>
		/// Opens the tunnel through rock only, so the rooms it joins keep their
		/// ledges, platforms and boss cells. Each end and bend gets a round cap.
		/// </summary>
		public void Carve(RiftGrid grid, int ox, int oy)
		{
			foreach (RiftSegment segment in Segments) {
				grid.Beam(segment.X0 + ox, segment.Y0 + oy, segment.X1 + ox, segment.Y1 + oy, segment.Thick, RiftGrid.Air, rockOnly: true);
				grid.Disk(segment.X0 + ox, segment.Y0 + oy, segment.Thick / 2.0, RiftGrid.Air, rockOnly: true);
				grid.Disk(segment.X1 + ox, segment.Y1 + oy, segment.Thick / 2.0, RiftGrid.Air, rockOnly: true);
			}
		}

		/// <summary>Platform rungs across every steep run. Call after all carving.</summary>
		public void Rungs(RiftGrid grid, int ox, int oy)
		{
			foreach (RiftSegment segment in Segments) {
				if (!segment.Steep) {
					continue;
				}

				double x0 = segment.X0 + ox;
				double y0 = segment.Y0 + oy;
				double x1 = segment.X1 + ox;
				double y1 = segment.Y1 + oy;
				double length = segment.Length;
				double c = (x1 - x0) / length;
				double s = (y1 - y0) / length;
				double halfT = segment.Thick / 2.0;
				int topFloor = (int)Math.Round(Math.Min(y0, y1) + halfT);
				int bottomFloor = (int)Math.Round(Math.Max(y0, y1) + halfT);
				int left = (int)Math.Floor(Math.Min(x0, x1) - segment.Thick) - 1;
				int right = (int)Math.Ceiling(Math.Max(x0, x1) + segment.Thick) + 1;
				for (int y = bottomFloor - RungPitch; y > topFloor; y -= RungPitch) {
					for (int x = left; x <= right; x++) {
						double dx = x + 0.5 - x0;
						double dy = y + 0.5 - y0;
						double along = dx * c + dy * s;
						double across = -dx * s + dy * c;
						if (along < 0 || along > length || Math.Abs(across) > halfT) {
							continue;
						}

						if (grid.Get(x, y) == RiftGrid.Air) {
							grid.Set(x, y, RiftGrid.Platform);
						}
					}
				}
			}
		}
	}
}
