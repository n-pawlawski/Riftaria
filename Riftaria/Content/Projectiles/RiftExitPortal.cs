using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Systems;

namespace Riftaria.Content.Projectiles
{
	/// <summary>
	/// Opens when the boss dies. Walking into it leaves the rift.
	/// </summary>
	public class RiftExitPortal : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DemonScythe;

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 60 * 20;
			Projectile.aiStyle = 0;
			Projectile.scale = 2f;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft = 60 * 60 * 20;
			Projectile.rotation += 0.08f;
			Lighting.AddLight(Projectile.Center, 0.45f, 0.2f, 0.7f);

			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch);
				dust.velocity = Projectile.Center - dust.position;
				dust.velocity *= 0.04f;
				dust.noGravity = true;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient || !RiftGateway.InRift) {
				return;
			}

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				if (Vector2.Distance(player.Center, Projectile.Center) > 36f) {
					continue;
				}

				RiftGateway.ReturnToHub();
				Projectile.Kill();
				return;
			}
		}
	}
}
