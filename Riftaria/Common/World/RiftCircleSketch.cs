using System;

namespace Riftaria.Common.World
{
	/// <summary>
	/// The circle rift as a tile grid. Built from <see cref="RiftCirclePlan"/> and
	/// <see cref="RiftCircleFloors"/>, which is what the game carves.
	/// </summary>
	public static class RiftCircleSketch
	{
		public const byte Rock = 0;
		public const byte Air = 1;
		public const byte Boss = 2;
		public const byte Floor = 3;

		public static int Width { get; private set; }

		public static int Height { get; private set; }

		public static byte[] Cells { get; private set; }

		public static void Roll()
		{
			RiftCirclePlan.Roll();
			Width = RiftCirclePlan.RequiredWidth();
			Height = RiftMeasures.CircleHeight;
			var rand = new Random();
			int[] floors = RiftCircleFloors.Build(Width, RiftMeasures.ForestRange, (min, max) => rand.Next(min, max));
			var rooms = RiftCircleFloors.MakeRooms(floors);
			Cells = new byte[Width * Height];
			if (rooms.Count == 0) {
				return;
			}

			int left = rooms[0].Left;
			int right = rooms[0].Right;
			foreach (BiomeRoom room in rooms) {
				if (room.Left < left) {
					left = room.Left;
				}

				if (room.Right > right) {
					right = room.Right;
				}
			}

			left = left < 0 ? 0 : left;
			right = right >= Width ? Width - 1 : right;
			BiomeRoom boss = rooms[rooms.Count - 1];
			int border = RiftMeasures.Border;
			for (int x = left; x <= right; x++) {
				int top = RiftCircleFloors.ColumnTop(x, floors, rooms);
				int floor = floors[x];
				bool arena = x >= boss.Left && x <= boss.Right;
				for (int y = top; y < floor; y++) {
					if (x < border || y < border || x >= Width - border || y >= Height - border) {
						continue;
					}

					Cells[y * Width + x] = arena ? Boss : Air;
				}

				if (floor >= border && floor < Height - border && x >= border && x < Width - border) {
					Cells[floor * Width + x] = Floor;
				}
			}
		}
	}
}
