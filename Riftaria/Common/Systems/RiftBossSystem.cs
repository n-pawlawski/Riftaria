using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Bosses;

namespace Riftaria.Common.Systems
{
	/// <summary>
	/// Health bars and triggers for every NPC registered as a boss. The bar is
	/// Terraria's, so it sits at the bottom of the screen. Triggers run once
	/// the condition becomes true.
	/// </summary>
	public class RiftBossSystem : ModSystem
	{
		private readonly Dictionary<int, FightProgress> fights = new();

		private readonly List<int> stale = new();

		public override void Unload()
		{
			fights.Clear();
		}

		public override void PostUpdateNPCs()
		{
			if (!RiftGateway.InRift) {
				fights.Clear();
				return;
			}

			ForgetInactive();

			bool canRunEvents = Main.netMode != NetmodeID.MultiplayerClient;
			bool canShowBar = Main.netMode != NetmodeID.Server;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !RiftBossRegistry.TryGet(npc.type, out RiftBoss boss)) {
					continue;
				}

				if (canShowBar && boss.ShowHealthBar && npc.life > 0) {
					npc.boss = true;
				}

				if (canRunEvents) {
					RunTriggers(npc, boss);
				}
			}
		}

		private void RunTriggers(NPC npc, RiftBoss boss)
		{
			if (!fights.TryGetValue(npc.whoAmI, out FightProgress progress) || progress.Type != npc.type) {
				progress = new FightProgress(npc.type, boss.Triggers.Count);
				fights[npc.whoAmI] = progress;
			}

			BossFight fight = new(npc, boss);
			for (int i = 0; i < boss.Triggers.Count; i++) {
				if (progress.Spent[i]) {
					continue;
				}

				BossTrigger trigger = boss.Triggers[i];
				bool met = trigger.Condition.IsMet(fight);
				if (met && !progress.WasMet[i]) {
					progress.WasMet[i] = true;
					if (trigger.Once) {
						progress.Spent[i] = true;
					}

					trigger.Action.Execute(fight);
				}
				else {
					progress.WasMet[i] = met;
				}
			}
		}

		private void ForgetInactive()
		{
			stale.Clear();
			foreach (KeyValuePair<int, FightProgress> pair in fights) {
				NPC npc = Main.npc[pair.Key];
				if (!npc.active || npc.type != pair.Value.Type) {
					stale.Add(pair.Key);
				}
			}

			foreach (int who in stale) {
				fights.Remove(who);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (index < 0) {
				return;
			}

			layers.Insert(index, new LegacyGameInterfaceLayer(
				"Riftaria: Boss Bar",
				DrawBossBar,
				InterfaceScaleType.UI));
		}

		/// <summary>
		/// Terraria's own boss bar. The game's tracker never starts it for an NPC
		/// that has no boss-head icon, so this draws that same bar while the boss lives.
		/// </summary>
		private static bool DrawBossBar()
		{
			if (!RiftGateway.InRift) {
				return true;
			}

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.life <= 0) {
					continue;
				}

				if (!RiftBossRegistry.TryGet(npc.type, out RiftBoss boss) || !boss.ShowHealthBar) {
					continue;
				}

				RiftBossBar.DrawFor(Main.spriteBatch, npc);
				break;
			}

			return true;
		}

		/// <summary>A little margin so the bar does not flicker as the boss clips the edge.</summary>
		internal static bool IsOnScreen(NPC npc)
		{
			Rectangle screen = new(
				(int)Main.screenPosition.X - 64,
				(int)Main.screenPosition.Y - 64,
				Main.screenWidth + 128,
				Main.screenHeight + 128);
			return npc.Hitbox.Intersects(screen);
		}

		private sealed class FightProgress
		{
			public int Type { get; }

			public bool[] Spent { get; }

			public bool[] WasMet { get; }

			public FightProgress(int type, int triggerCount)
			{
				Type = type;
				Spent = new bool[triggerCount];
				WasMet = new bool[triggerCount];
			}
		}
	}
}
