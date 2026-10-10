using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Spawns copies of an NPC across the fight.
	/// The first stands just inside the screen edge on the player's side of the
	/// boss and faces the player. The second stands on the opposite side of the
	/// boss, half as far from the boss as the first one is. Any further copies
	/// pick a random spot between those two, on the same ground.
	/// </summary>
	public sealed class SpawnAroundHorizontally : IBossAction
	{
		private const int EdgeInset = 32;

		private readonly int npcType;

		private readonly int count;

		public SpawnAroundHorizontally(int npcType, int count)
		{
			this.npcType = npcType;
			this.count = count;
		}

		public void Execute(BossFight fight)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || count <= 0) {
				return;
			}

			NPC boss = fight.Boss;
			Player player = TargetOf(boss);
			if (player == null) {
				return;
			}

			int bossX = (int)boss.Center.X;
			int bossFeet = (int)boss.Bottom.Y;
			var source = new EntitySource_Parent(boss);

			if (!TryPlaceNearEdge(bossX, bossFeet, player, out int firstX, out int firstFeet)) {
				return;
			}

			Spawn(source, firstX, firstFeet, player);

			if (count < 2) {
				return;
			}

			int distance = Math.Abs(bossX - firstX);
			int towardOpposite = bossX >= firstX ? 1 : -1;
			int secondX = bossX + towardOpposite * (distance / 2);
			int secondFeet = FeetOn(secondX, bossFeet);
			Spawn(source, secondX, secondFeet, player);

			int left = Math.Min(firstX, secondX);
			int right = Math.Max(firstX, secondX);
			for (int i = 2; i < count; i++) {
				if (right - left <= 16) {
					break;
				}

				int x = Main.rand.Next(left + 8, right - 7);
				Spawn(source, x, FeetOn(x, bossFeet), player);
			}
		}

		/// <summary>
		/// The screen edge on the player's side of the boss, walked inward until
		/// there is floor to stand on.
		/// </summary>
		private static bool TryPlaceNearEdge(int bossX, int bossFeet, Player player, out int worldX, out int feet)
		{
			float zoom = Main.GameViewMatrix.Zoom.X;
			if (zoom < 0.01f) {
				zoom = 1f;
			}

			float viewWidth = Main.screenWidth / zoom;
			float left = Main.screenPosition.X + (Main.screenWidth - viewWidth) / 2f;
			float right = left + viewWidth;
			bool playerOnLeft = player.Center.X <= bossX;
			int desired = (int)(playerOnLeft ? left + EdgeInset : right - EdgeInset);
			int step = bossX >= desired ? 16 : -16;

			worldX = desired;
			for (int guard = 0; guard < 240; guard++) {
				if (TryStand(worldX, bossFeet, out feet)) {
					return true;
				}

				if (Math.Abs(worldX - bossX) <= 16) {
					break;
				}

				worldX += step;
			}

			feet = bossFeet;
			return TryStand(bossX, bossFeet, out feet);
		}

		private static int FeetOn(int worldX, int nearFeet)
		{
			if (TryStand(worldX, nearFeet, out int feet)) {
				return feet;
			}

			return nearFeet;
		}

		private static bool TryStand(int worldX, int nearFeet, out int feet)
		{
			int tileX = worldX / 16;
			int start = nearFeet / 16;
			for (int dy = 0; dy <= 48; dy++) {
				if (IsSurface(tileX, start + dy, out feet)) {
					return true;
				}

				if (dy > 0 && IsSurface(tileX, start - dy, out feet)) {
					return true;
				}
			}

			feet = nearFeet;
			return false;
		}

		private static bool IsSurface(int tileX, int tileY, out int feet)
		{
			feet = 0;
			if (!WorldGen.InWorld(tileX, tileY) || !WorldGen.InWorld(tileX, tileY - 1)) {
				return false;
			}

			Tile floor = Main.tile[tileX, tileY];
			Tile above = Main.tile[tileX, tileY - 1];
			if (!floor.HasTile || above.HasTile || !Main.tileSolid[floor.TileType]) {
				return false;
			}

			feet = tileY * 16;
			return true;
		}

		private static Player TargetOf(NPC boss)
		{
			if (boss.HasPlayerTarget) {
				Player aimed = Main.player[boss.target];
				if (aimed.active && !aimed.dead) {
					return aimed;
				}
			}

			Player nearest = null;
			float best = float.MaxValue;
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				float dist = Math.Abs(player.Center.X - boss.Center.X);
				if (dist < best) {
					best = dist;
					nearest = player;
				}
			}

			return nearest;
		}

		private void Spawn(EntitySource_Parent source, int x, int feet, Player player)
		{
			if (!WorldGen.InWorld(x / 16, feet / 16)) {
				return;
			}

			int who = NPC.NewNPC(source, x, feet, npcType);
			if (who < 0 || who >= Main.maxNPCs) {
				return;
			}

			NPC npc = Main.npc[who];
			int dir = player.Center.X >= npc.Center.X ? 1 : -1;
			npc.direction = dir;
			npc.spriteDirection = dir;
			npc.target = player.whoAmI;
		}
	}
}
