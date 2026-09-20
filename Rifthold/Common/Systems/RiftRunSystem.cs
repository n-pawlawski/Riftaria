using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Net;
using Rifthold.Common.Rewards;
using Rifthold.Common.World;
using Rifthold.Content.NPCs;

namespace Rifthold.Common.Systems
{
	/// <summary>Drives a rift run: spawning the objective, detecting the clear, going home.</summary>
	public class RiftRunSystem : ModSystem
	{
		/// <summary>How close a player must get before the Warden manifests.</summary>
		private const float WardenTriggerRange = 900f;

		/// <summary>Ticks between the clear and the automatic return.</summary>
		private const int ExitDelayTicks = 60 * 12;

		public override void PostUpdateWorld()
		{
			if (!RiftGateway.InRift) {
				return;
			}

			TrySpawnWarden();
		}

		public override void PostUpdateEverything()
		{
			if (!RiftGateway.InRift || Main.netMode == NetmodeID.Server) {
				return;
			}

			TickExit();
		}

		/// <summary>
		/// The Warden is spawned on approach rather than at generation time, so it
		/// is not burning its charge cycle in an empty room while the player walks.
		/// </summary>
		private static void TrySpawnWarden()
		{
			if (RiftLayout.WardenSpawned || Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			Vector2 arena = RiftLayout.ArenaCenter.ToWorldCoordinates();

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				if (Vector2.Distance(player.Center, arena) > WardenTriggerRange) {
					continue;
				}

				RiftLayout.WardenSpawned = true;
				NPC.NewNPC(new Terraria.DataStructures.EntitySource_SpawnNPC(),
					(int)arena.X, (int)arena.Y, ModContent.NPCType<RiftWarden>());
				return;
			}
		}

		/// <summary>Called when the Warden dies. Runs on the server, or in single player.</summary>
		public static void CompleteRun()
		{
			if (RiftRunState.ObjectiveComplete) {
				return;
			}

			RiftRunState.ObjectiveComplete = true;
			RiftRunState.ExitCountdown = ExitDelayTicks;

			int tier = RiftRunState.ActiveTier;
			RiftRewards.GrantToEveryoneInRift(tier);

			Announce(Language.GetTextValue("Mods.Rifthold.Rift.Cleared"), new Color(150, 255, 180));

			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = ModContent.GetInstance<Rifthold>().GetPacket();
				packet.Write((byte)RiftholdPacketType.RunComplete);
				packet.Write(tier);
				packet.Send();
			}
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
