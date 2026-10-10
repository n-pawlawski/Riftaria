using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Riftaria.Content.Projectiles
{
	/// <summary>A rock thrown on an arc. Gravity is shared with the throw so the aim lands.</summary>
	public class RiftBoulder : ModProjectile
	{
		public const float Gravity = 0.22f;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Boulder;

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.scale = 0.55f;
		}

		public override void AI()
		{
			Projectile.velocity.Y += Gravity;
			if (Projectile.velocity.Y > 16f) {
				Projectile.velocity.Y = 16f;
			}

			Projectile.rotation += 0.15f * (Projectile.velocity.X >= 0f ? 1f : -1f);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => true;
	}
}
