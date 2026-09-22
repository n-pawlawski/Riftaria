using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Content.Tiles;

namespace Riftaria.Content.Items.Placeable
{
	public class MapDeviceItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<MapDeviceTile>());
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			// No crafting station: the Hold starts with one placed, and a spare
			// should not depend on furniture the player cannot mine materials for.
			CreateRecipe()
				.AddIngredient<RiftEssence>(8)
				.Register();
		}
	}
}
