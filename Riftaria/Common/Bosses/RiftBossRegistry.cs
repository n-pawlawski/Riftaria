using System;
using System.Collections.Generic;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// One configured boss: the NPC type, whether it draws the health bar, and
	/// the triggers that can fire during its fight.
	/// </summary>
	public sealed class RiftBoss
	{
		public int NpcType { get; }

		/// <summary>
		/// Terraria's boss health bar, drawn at the bottom of the screen while
		/// this NPC is on screen.
		/// </summary>
		public bool ShowHealthBar { get; set; } = true;

		public List<BossTrigger> Triggers { get; } = new();

		/// <summary>
		/// Movement, in order. Each one runs until it finishes, then the next
		/// starts. After the last, the first starts again.
		/// </summary>
		public List<IBossMotion> Motions { get; } = new();

		public RiftBoss(int npcType)
		{
			NpcType = npcType;
		}

		public void Moves(params IBossMotion[] motions)
		{
			Motions.AddRange(motions);
		}

		/// <summary>
		/// Fires <paramref name="then"/> on the tick <paramref name="when"/>
		/// becomes true. <paramref name="once"/> spends the trigger for the
		/// rest of this boss's life. Leave it false to arm the trigger again
		/// each time the condition goes false and then true.
		/// </summary>
		public void When(IBossCondition when, IBossAction then, bool once = true)
		{
			Triggers.Add(new BossTrigger(when, then, once));
		}
	}

	public sealed class BossTrigger
	{
		public IBossCondition Condition { get; }

		public IBossAction Action { get; }

		public bool Once { get; }

		public BossTrigger(IBossCondition condition, IBossAction action, bool once)
		{
			Condition = condition;
			Action = action;
			Once = once;
		}
	}

	/// <summary>Bosses registered from an NPC's <c>SetStaticDefaults</c>.</summary>
	public static class RiftBossRegistry
	{
		private static readonly Dictionary<int, RiftBoss> ByType = new();

		public static void Define(int npcType, Action<RiftBoss> configure)
		{
			RiftBoss boss = new(npcType);
			configure(boss);
			ByType[npcType] = boss;
		}

		public static bool TryGet(int npcType, out RiftBoss boss)
			=> ByType.TryGetValue(npcType, out boss);

		public static void Reset() => ByType.Clear();
	}
}
