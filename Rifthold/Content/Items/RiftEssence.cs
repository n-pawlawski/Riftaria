using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rifthold.Content.Items
{
	/// <summary>Currency scraped out of a cleared rift.</summary>
	public class RiftEssence : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 9999;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Green;
		}
	}
}
