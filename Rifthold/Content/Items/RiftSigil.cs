using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Rifthold.Common.Maps;

namespace Rifthold.Content.Items
{
	/// <summary>
	/// A key to one rift. The tier lives on the item instance rather than in the
	/// item type, so a single class covers every tier and new tiers need no new
	/// content ids.
	/// </summary>
	public class RiftSigil : ModItem
	{
		public int Tier = RiftTierRegistry.FirstTier;

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			// Instanced data means these must never stack; two tiers merging into
			// one slot would silently destroy a sigil.
			Item.maxStack = 1;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.LightPurple;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.consumable = false;
		}

		/// <summary>Builds a sigil item of the given tier, ready to spawn or hand out.</summary>
		public static Item WithTier(int tier)
		{
			Item item = new();
			item.SetDefaults(ModContent.ItemType<RiftSigil>());

			if (item.ModItem is RiftSigil sigil) {
				sigil.Tier = tier;
			}

			return item;
		}

		public override void SaveData(TagCompound tag) => tag["tier"] = Tier;

		public override void LoadData(TagCompound tag)
			=> Tier = tag.ContainsKey("tier") ? tag.GetInt("tier") : RiftTierRegistry.FirstTier;

		public override void NetSend(BinaryWriter writer) => writer.Write(Tier);

		public override void NetReceive(BinaryReader reader) => Tier = reader.ReadInt32();

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			RiftTier data = RiftTierRegistry.Get(Tier);

			tooltips.Insert(1, new TooltipLine(Mod, "RiftTier",
				Language.GetTextValue("Mods.Rifthold.Rift.Title", Tier)));

			tooltips.Insert(2, new TooltipLine(Mod, "RiftRequirement",
				Language.GetTextValue("Mods.Rifthold.Rift.Requirement", data.RequiredLevel)));
		}
	}
}
