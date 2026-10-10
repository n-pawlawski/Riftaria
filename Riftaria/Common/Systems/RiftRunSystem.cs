using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.Net;
using Riftaria.Common.World;
using Riftaria.Content.NPCs;
using Riftaria.Content.Projectiles;

namespace Riftaria.Common.Systems
{
	/// <summary>Drives a rift run: spawning the objective, detecting the clear, going home.</summary>
	public class RiftRunSystem : ModSystem
	{
		/// <summary>How close a player must get before a path enemy manifests.</summary>
		private const float PathTriggerRange = 480f;

		public override void PostUpdateWorld()
		{
			if (!RiftGateway.InRift) {
				return;
			}

			TrySpawnPathEnemies();
			TrySpawnObjective();
		}

		public override void PostUpdateEverything()
		{
			if (!RiftGateway.InRift || Main.netMode == NetmodeID.Server) {
				return;
			}

			TickExit();
		}

		/// <summary>
		/// Path enemies spawn on approach, same as the objective, so a slime at
		/// the far end is not despawned before the player reaches that circle.
		/// </summary>
		private static void TrySpawnPathEnemies()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			foreach (RiftPathEnemy enemy in RiftLayout.PathEnemies) {
				if (enemy.Spawned) {
					continue;
				}

				// Feet go on the top of the tile. The center of a solid tile puts the
				// hitbox inside the floor, and a fighter then turns around for good.
				Vector2 at = enemy.Tile.ToWorldCoordinates(8f, 0f);
				if (!PlayerIsNear(at, PathTriggerRange)) {
					continue;
				}

				int who = NPC.NewNPC(new Terraria.DataStructures.EntitySource_SpawnNPC(),
					(int)at.X, (int)at.Y, enemy.NpcType);
				enemy.Spawned = who >= 0 && who < Main.maxNPCs;
			}
		}

		private static bool PlayerIsNear(Vector2 point, float range)
		{
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				if (Vector2.Distance(player.Center, point) <= range) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// The boss stays unspawned until every living player is inside the chamber.
		/// </summary>
		private static void TrySpawnObjective()
		{
			if (RiftLayout.ObjectiveSpawned || Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			if (BossRoom.IsDefined) {
				if (!BossRoom.HoldsEveryone()) {
					return;
				}

				SpawnObjective();
				return;
			}
		}

		private static void SpawnObjective()
		{
			Vector2 arena = RiftLayout.ArenaCenter.ToWorldCoordinates();
			int type = RiftLayout.ObjectiveNpc;
			if (type <= 0) {
				type = ModContent.NPCType<RiftZombie>();
			}

			int spawnX = (int)arena.X;
			int spawnY = (int)arena.Y;
			if (RiftLayout.ObjectiveFeetX >= 0) {
				spawnX = RiftLayout.ObjectiveFeetX * 16 + 8;
				spawnY = RiftLayout.ObjectiveFeetY * 16;
			}

			int who = NPC.NewNPC(new EntitySource_SpawnNPC(), spawnX, spawnY, type);
			RiftLayout.ObjectiveSpawned = who >= 0 && who < Main.maxNPCs;
			RiftLayout.ObjectiveWho = RiftLayout.ObjectiveSpawned ? who : -1;
		}

		/// <summary>Called when the boss dies. Runs on the server, or in single player.</summary>
		public static void CompleteRun()
		{
			if (RiftRunState.ObjectiveComplete) {
				return;
			}

			RiftRunState.ObjectiveComplete = true;
			RiftRunState.ExitCountdown = -1;
			if (!RiftGateway.InRift) {
				return;
			}

			Announce(Language.GetTextValue("Mods.Riftaria.Rift.Cleared"), new Color(150, 255, 180));
			OpenPortal();

			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = ModContent.GetInstance<Riftaria>().GetPacket();
				packet.Write((byte)RiftariaPacketType.RunComplete);
				packet.Write(RiftRunState.ActiveTier);
				packet.Send();
			}
		}

		private static void OpenPortal()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || RiftLayout.PortalX < 0) {
				return;
			}

			int x = RiftLayout.PortalX * 16 + 8;
			int y = RiftLayout.PortalY * 16 + 8;
			Projectile.NewProjectile(
				new EntitySource_WorldEvent(),
				x,
				y,
				0f,
				0f,
				ModContent.ProjectileType<RiftExitPortal>(),
				0,
				0f,
				Main.myPlayer);
		}

		/// <summary>Client-side countdown that walks the player back to the Hold.</summary>
		private static void TickExit()
		{
			if (RiftRunState.ExitCountdown < 0) {
				return;
			}

			RiftRunState.ExitCountdown--;

			if (RiftRunState.ExitCountdown <= 0) {
				RiftRunState.ExitCountdown = -1;
				RiftGateway.ReturnToHub();
			}
		}

		internal static void Announce(string message, Color colour)
		{
			if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), colour);
			}
			else {
				Main.NewText(message, colour);
			}
		}
	}
}
