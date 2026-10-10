using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Riftaria.Content.NPCs;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Holds still and spins, releasing smaller NPCs around itself.
	/// Spawning stops once the living children reach maxAlive.
	/// </summary>
	public sealed class Brood : IBossMotion
	{
		private readonly int npcType;

		private readonly int count;

		private readonly int ticks;

		private readonly float spinPerTick;

		private readonly int maxAlive;

		public Brood(int npcType, int count, int ticks, float spinPerTick, int maxAlive = 0)
		{
			this.npcType = npcType;
			this.count = count;
			this.ticks = ticks;
			this.spinPerTick = spinPerTick;
			this.maxAlive = maxAlive > 0 ? maxAlive : count * 2;
		}

		public bool Step(NPC npc)
		{
			npc.velocity = Vector2.Zero;
			npc.rotation += spinPerTick;

			if (Main.netMode != NetmodeID.MultiplayerClient && count > 0 && BossSpawnedNpc.CountOf(npc) < maxAlive) {
				int every = Math.Max(1, ticks / count);
				int n = (int)npc.ai[1] - 1;
				if (n >= 0 && n % every == 0 && n / every < count) {
					Release(npc, n / every);
				}
			}

			return npc.ai[1] >= ticks;
		}

		private void Release(NPC npc, int index)
		{
			float angle = MathHelper.TwoPi * index / count;
			Vector2 at = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 40f;
			int who = NPC.NewNPC(new EntitySource_Parent(npc), (int)at.X, (int)at.Y, npcType);
			if (who < 0 || who >= Main.maxNPCs) {
				return;
			}

			NPC child = Main.npc[who];
			child.velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
			child.timeLeft = 3600;
			child.target = npc.target;
		}
	}
}
