using System;
using System.Collections.Generic;

namespace Riftaria.Common.World
{
	/// <summary>
	/// The walk under a circle rift. The game builds rooms from this, and the
	/// preview draws the same floors.
	/// </summary>
	public static class RiftCircleFloors
	{
		public static int[] Build(int width, int verticalRange, Func<int, int, int> next)
		{
			int[] centers = RiftCirclePlan.Centers;
			int[] radii = RiftCirclePlan.Radii;
			int[] floors = new int[width];
			int baseline = RiftMeasures.CircleFloorY;
			for (int x = 0; x < floors.Length; x++) {
				floors[x] = baseline;
			}

			if (centers == null || centers.Length == 0) {
				return floors;
			}

			int minY = RiftMeasures.Border + 16;
			int maxY = RiftMeasures.CircleHeight - RiftMeasures.Border - 4;
			int[] targets = new int[centers.Length];
			int drift = 0;
			for (int i = 0; i < centers.Length; i++) {
				if (verticalRange > 0) {
					drift += next(-3, 4);
					drift = Clamp(drift, -verticalRange, verticalRange);
				}

				targets[i] = Clamp(baseline + drift, minY, maxY);
			}

			for (int i = 0; i < centers.Length - 1; i++) {
				int x0 = centers[i];
				int x1 = centers[i + 1];
				int y0 = targets[i];
				int y1 = targets[i + 1];
				int span = Math.Max(1, x1 - x0);
				for (int x = x0; x <= x1 && x < floors.Length; x++) {
					if (x < 0) {
						continue;
					}

					floors[x] = y0 + ((y1 - y0) * (x - x0) / span);
				}
			}

			int start = centers[0] - radii[0];
			int end = centers[0] + radii[0];
			for (int i = 0; i < centers.Length; i++) {
				int left = centers[i] - radii[i];
				int right = centers[i] + radii[i];
				if (left < start) {
					start = left;
				}

				if (right > end) {
					end = right;
				}
			}

			start = Clamp(start, 0, floors.Length - 1);
			end = Clamp(end, 0, floors.Length - 1);
			int firstCenter = Clamp(centers[0], 0, floors.Length - 1);
			int lastCenter = Clamp(centers[centers.Length - 1], 0, floors.Length - 1);
			for (int x = start; x < firstCenter; x++) {
				floors[x] = targets[0];
			}

			for (int x = lastCenter; x <= end; x++) {
				floors[x] = targets[targets.Length - 1];
			}

			Constrain(floors, start, start, end);
			Constrain(floors, start, end, start);
			Constrain(floors, start, start, end);
			FlattenStand(floors, centers[centers.Length - 1]);
			return floors;
		}

		public static List<BiomeRoom> MakeRooms(int[] floors)
		{
			int[] centers = RiftCirclePlan.Centers;
			int[] radii = RiftCirclePlan.Radii;
			var rooms = new List<BiomeRoom>(centers.Length);
			for (int i = 0; i < centers.Length; i++) {
				int center = Clamp(centers[i], 0, floors.Length - 1);
				int floor = floors[center];
				int circleY = floor - (RiftMeasures.PassageHeight / 2);
				int crown = circleY - radii[i];
				if (crown < RiftMeasures.Border + 1) {
					crown = RiftMeasures.Border + 1;
				}

				rooms.Add(new BiomeRoom {
					Left = centers[i] - radii[i],
					Right = centers[i] + radii[i],
					Height = radii[i] + (RiftMeasures.PassageHeight / 2),
					Ceiling = crown,
					Floors = floors,
					Round = true,
					Dress = i < centers.Length - 1,
					Radius = radii[i],
					CircleCenterY = circleY
				});
			}

			return rooms;
		}

		/// <summary>First open row in this column. The passage is always open, and a circle can raise it.</summary>
		public static int ColumnTop(int x, int[] floors, List<BiomeRoom> rooms)
		{
			int top = floors[x] - RiftMeasures.PassageHeight;
			foreach (BiomeRoom room in rooms) {
				if (x < room.Left || x > room.Right) {
					continue;
				}

				int circle = room.CeilingAt(x);
				if (circle < top) {
					top = circle;
				}
			}

			int limit = RiftMeasures.Border + 1;
			return top < limit ? limit : top;
		}

		public static int Ceiling(int x, int center, int radius, int circleCenterY)
		{
			int dx = x - center;
			int riseSq = (radius * radius) - (dx * dx);
			int top = circleCenterY;
			if (riseSq > 0) {
				top = circleCenterY - (int)Math.Sqrt(riseSq);
			}

			int limit = RiftMeasures.Border + 1;
			return top < limit ? limit : top;
		}

		private static void Constrain(int[] floors, int origin, int from, int to)
		{
			int step = from < to ? 1 : -1;
			for (int x = from + step; x != to + step; x += step) {
				int neighbour = x - step;
				int higher = x > neighbour ? x : neighbour;
				int delta = (higher - origin) % 2 == 0 ? 1 : 0;
				floors[x] = Clamp(floors[x], floors[neighbour] - delta, floors[neighbour] + delta);
			}
		}

		private static void FlattenStand(int[] floors, int center)
		{
			center = Clamp(center, 1, floors.Length - 2);
			int flat = floors[center];
			int padLeft = Clamp(center - 4, 0, floors.Length - 1);
			int padRight = Clamp(center + 8, 0, floors.Length - 1);
			for (int x = padLeft; x <= padRight; x++) {
				floors[x] = flat;
			}

			for (int x = padLeft - 1; x >= 1; x--) {
				int above = floors[x + 1] + 1;
				int below = floors[x + 1] - 1;
				if (floors[x] > above) {
					floors[x] = above;
				}
				else if (floors[x] < below) {
					floors[x] = below;
				}
				else {
					break;
				}
			}
		}

		private static int Clamp(int value, int min, int max)
			=> value < min ? min : value > max ? max : value;
	}
}
