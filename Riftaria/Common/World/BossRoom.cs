using Terraria;

namespace Riftaria.Common.World
{
	/// <summary>
	/// Shared rules for a boss chamber. A solid margin surrounds the room.
	/// The path may cross that margin only at the entrance. The boss waits
	/// until every living player is inside.
	/// </summary>
	public static class BossRoom
	{
		/// <summary>Tiles of solid ground around the chamber, apart from the entrance.</summary>
		public const int Buffer = RiftMeasures.Buffer;

		public static bool IsDefined => RiftLayout.ObjectiveRoomLeft >= 0;

		public static bool Contains(Player player)
		{
			int x = (int)(player.Center.X / 16f);
			int y = (int)(player.Center.Y / 16f);
			return x >= RiftLayout.ObjectiveRoomLeft
				&& x < RiftLayout.ObjectiveRoomRight
				&& y >= RiftLayout.ObjectiveRoomTop
				&& y < RiftLayout.ObjectiveRoomBottom;
		}

		/// <summary>Every living player is standing in the chamber. Dead players do not count.</summary>
		public static bool HoldsEveryone()
		{
			bool any = false;
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				any = true;
				if (!Contains(player)) {
					return false;
				}
			}

			return any;
		}
	}
}
