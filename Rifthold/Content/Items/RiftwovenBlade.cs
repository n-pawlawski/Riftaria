using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rifthold.Content.Items
{
	/// <summary>
	/// The tier 1 completion weapon. With ore progression gone, rift rewards are
	/// the only way gear enters the game at all.
	/// </summary>
	public class RiftwovenBlade : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 5f;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
	}
}
