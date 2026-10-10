using System;
using Terraria;
using Terraria.ModLoader;
using Riftaria.Common.Items;

namespace Riftaria.Common.Players
{
	/// <summary>
	/// Global stats summed from equipped armor and accessories. The sheet is
	/// cleared every tick and refilled by <c>RiftAffixItem.UpdateEquip</c>, then
	/// applied to the player and to every weapon.
	/// </summary>
	public class RiftStatPlayer : ModPlayer
	{
		private readonly int[] _sheet = new int[Stats.Count];

		public int this[StatId stat] => _sheet[(int)stat];

		public void Add(StatId stat, int value) => _sheet[(int)stat] += value;

		public override void ResetEffects() => Array.Clear(_sheet);

		public override void PostUpdateEquips()
		{
			Player.statLifeMax2 += this[StatId.MaxLife];
			Player.statManaMax2 += this[StatId.MaxMana];
			Player.lifeRegen += this[StatId.LifeRegen];
			Player.statDefense += this[StatId.Defense];
			Player.moveSpeed += this[StatId.MoveSpeed] / 100f;
			Player.endurance += this[StatId.DamageReduction] / 100f;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) => damage += this[StatId.IncreasedDamage] / 100f;

		public override void ModifyWeaponCrit(Item item, ref float crit) => crit += this[StatId.Crit];

		public override float UseSpeedMultiplier(Item item) => 1f + this[StatId.AttackSpeed] / 100f;
	}
}
