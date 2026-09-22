using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Riftaria.Common.Systems;
using Riftaria.Content.Items.Placeable;
using Riftaria.UI;

namespace Riftaria.Content.Tiles
{
	/// <summary>The Rift Gate. Right clicking it in the Hold opens the run selector.</summary>
	public class MapDeviceTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(90, 140, 200), CreateMapEntryName());

			DustType = DustID.Granite;
			HitSound = SoundID.Tink;
		}

		public override bool RightClick(int i, int j)
		{
			if (RiftGateway.InRift) {
				Main.NewText(Language.GetTextValue("Mods.Riftaria.Rift.NotInHold"), 255, 140, 140);
				return true;
			}

			ModContent.GetInstance<RiftGateUISystem>().Toggle();
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<MapDeviceItem>();
		}
	}
}
