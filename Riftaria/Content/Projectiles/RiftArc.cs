using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Riftaria.Content.Projectiles
{
	/// <summary>A bone thrown on an arc. Gravity is shared with the throw so the aim lands.</summary>
	public class RiftArc : ModProjectile
	{
		public const float Gravity = 0.18f;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bone;

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
		}

		public override void AI()
		{
			Projectile.velocity.Y += Gravity;
			if (Projectile.velocity.Y > 14f) {
				Projectile.velocity.Y = 14f;
			}

			Projectile.rotation += 0.2f * (Projectile.velocity.X >= 0f ? 1f : -1f);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => true;
	}
}
