using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Riftaria.Content.Items
{
	/// <summary>
	/// Stats from the old fixed loot rows. Kept so items that already dropped
	/// still load and work. New drops use <see cref="RiftAffixItem"/>.
	/// </summary>
	public class RiftRolledItem : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public bool Rolled { get; private set; }

		public string RowName { get; private set; } = "";

		public float Damage { get; private set; }

		public float AttackSpeed { get; private set; }

		public int Crit { get; private set; }

		public float ArrowSpeed { get; private set; }

		public override GlobalItem Clone(Item from, Item to)
		{
			RiftRolledItem clone = (RiftRolledItem)base.Clone(from, to);
			clone.Rolled = Rolled;
			clone.RowName = RowName;
			clone.Damage = Damage;
			clone.AttackSpeed = AttackSpeed;
			clone.Crit = Crit;
			clone.ArrowSpeed = ArrowSpeed;
			return clone;
		}

		public override void UpdateInventory(Item item, Player player) => ApplyName(item);

		public override void HoldItem(Item item, Player player) => ApplyName(item);

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
		{
			if (Rolled && Damage != 0f) {
				damage *= 1f + Damage;
			}
		}

		public override float UseSpeedMultiplier(Item item, Player player)
		{
			if (!Rolled || AttackSpeed == 0f) {
				return 1f;
			}

			return 1f + AttackSpeed;
		}

		public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
		{
			if (Rolled && Crit != 0) {
				crit += Crit;
			}
		}

		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (Rolled && ArrowSpeed != 0f) {
				velocity *= 1f + ArrowSpeed;
			}
		}

		public override void ModifyTooltips(Item item, System.Collections.Generic.List<TooltipLine> tooltips)
		{
			if (!Rolled) {
				return;
			}

			if (!string.IsNullOrEmpty(RowName)) {
				foreach (TooltipLine line in tooltips) {
					if (line.Name == "ItemName") {
						line.Text = RowName + " " + Lang.GetItemNameValue(item.type);
						break;
					}
				}
			}

			string stats = Describe();
			if (stats.Length > 0) {
				tooltips.Add(new TooltipLine(Mod, "RiftRoll", stats));
			}
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (!Rolled) {
				return;
			}

			tag["name"] = RowName ?? "";
			tag["damage"] = Damage;
			tag["speed"] = AttackSpeed;
			tag["crit"] = Crit;
			tag["arrow"] = ArrowSpeed;
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			Rolled = true;
			RowName = tag.GetString("name");
			Damage = tag.GetFloat("damage");
			AttackSpeed = tag.GetFloat("speed");
			Crit = tag.GetInt("crit");
			ArrowSpeed = tag.GetFloat("arrow");
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(Rolled);
			if (!Rolled) {
				return;
			}

			writer.Write(RowName ?? "");
			writer.Write(Damage);
			writer.Write(AttackSpeed);
			writer.Write(Crit);
			writer.Write(ArrowSpeed);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			Rolled = reader.ReadBoolean();
			if (!Rolled) {
				return;
			}

			RowName = reader.ReadString();
			Damage = reader.ReadSingle();
			AttackSpeed = reader.ReadSingle();
			Crit = reader.ReadInt32();
			ArrowSpeed = reader.ReadSingle();
		}

		private void ApplyName(Item item)
		{
			if (!Rolled || string.IsNullOrEmpty(RowName)) {
				return;
			}

			item.SetNameOverride(RowName + " " + Lang.GetItemNameValue(item.type));
		}

		private string Describe()
		{
			var text = new StringBuilder();
			Append(text, Damage, "damage");
			Append(text, AttackSpeed, "attack speed");
			if (Crit != 0) {
				if (text.Length > 0) {
					text.Append(", ");
				}

				text.Append('+').Append(Crit).Append(" crit");
			}

			Append(text, ArrowSpeed, "arrow speed");
			return text.ToString();
		}

		private static void Append(StringBuilder text, float fraction, string label)
		{
			if (fraction == 0f) {
				return;
			}

			if (text.Length > 0) {
				text.Append(", ");
			}

			text.Append('+').Append((int)System.Math.Round(fraction * 100f)).Append("% ").Append(label);
		}
	}
}
