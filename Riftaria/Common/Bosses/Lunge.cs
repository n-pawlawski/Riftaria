using Microsoft.Xna.Framework;
using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Picks a player, then flies that direction until a solid tile is in front.
	/// The direction is locked on the first tick. Later movement does not steer.
	/// </summary>
	public sealed class Lunge : IBossMotion
	{
		private readonly float speed;

		public Lunge(float speed)
		{
			this.speed = speed;
		}

		public bool Step(NPC npc)
		{
			if (npc.ai[1] <= 1f) {
				npc.TargetClosest(faceTarget: false);
				Vector2 aim = Vector2.UnitX * (npc.direction == 0 ? 1 : npc.direction);
				if (npc.HasValidTarget) {
					aim = Main.player[npc.target].Center - npc.Center;
				}

				if (aim.LengthSquared() < 1f) {
					aim = Vector2.UnitX;
				}

				aim.Normalize();
				npc.ai[2] = aim.X;
				npc.ai[3] = aim.Y;
			}

			Vector2 direction = new(npc.ai[2], npc.ai[3]);
			if (direction.LengthSquared() < 0.01f) {
				direction = Vector2.UnitX;
			}

			npc.velocity = direction * speed;
			npc.rotation = 0f;
			int facing = npc.velocity.X >= 0f ? 1 : -1;
			npc.direction = facing;
			npc.spriteDirection = facing;

			// The first ticks are a grace period so a spawn overlapping a corner
			// does not end the lunge before it has left.
			if (npc.ai[1] <= 4f) {
				return false;
			}

			if (WallAhead(npc, direction)) {
				npc.velocity = Vector2.Zero;
				return true;
			}

			return false;
		}

		private static bool WallAhead(NPC npc, Vector2 direction)
		{
			Vector2 front = npc.Center + direction * (System.Math.Max(npc.width, npc.height) * 0.5f + 2f);
			Vector2 tangent = new(-direction.Y, direction.X);
			float span = System.Math.Min(npc.width, npc.height) * 0.35f;
			for (int i = -1; i <= 1; i++) {
				Vector2 point = front + tangent * (span * i);
				int tileX = (int)(point.X / 16f);
				int tileY = (int)(point.Y / 16f);
				if (!WorldGen.InWorld(tileX, tileY, 8)) {
					return true;
				}

				Tile tile = Main.tile[tileX, tileY];
				if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
					return true;
				}
			}

			return false;
		}
	}
}
