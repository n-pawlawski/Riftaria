using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>One living boss. Conditions and actions both read this.</summary>
	public sealed class BossFight
	{
		public NPC Boss { get; }

		public RiftBoss Definition { get; }

		public BossFight(NPC boss, RiftBoss definition)
		{
			Boss = boss;
			Definition = definition;
		}
	}

	/// <summary>
	/// Becomes true on the tick an event should fire. A new kind of moment
	/// (a timer, a place the player stands, a hit count) is a new class.
	/// </summary>
	public interface IBossCondition
	{
		bool IsMet(BossFight fight);
	}

	/// <summary>What a trigger does once its condition becomes true.</summary>
	public interface IBossAction
	{
		void Execute(BossFight fight);
	}
}
