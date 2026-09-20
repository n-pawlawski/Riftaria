using System.Globalization;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Players;
using Rifthold.Common.Progression;
using Rifthold.Common.Systems;
using Rifthold.Content.Items;

namespace Rifthold.Common.Commands
{
	/// <summary>
	/// Developer shortcuts. Every one of these exists because the alternative is
	/// replaying the loop from level 1 to check a change three tiers deep.
	/// </summary>
	public class RiftCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "rift";

		public override string Usage =>
			"/rift xp <amount> | level <n> | points <n> | sigil <tier> | enter <tier> | exit | hub <stage> | where";

		public override string Description => "Rifthold developer commands.";

		private static readonly Color Info = new(150, 200, 255);
		private static readonly Color Problem = new(255, 140, 140);

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length == 0) {
				caller.Reply(Usage, Info);
				return;
			}

			Player player = caller.Player;
			RiftholdPlayer progression = RiftholdPlayer.Get(player);

			switch (args[0].ToLowerInvariant()) {
				case "xp":
					if (!TryNumber(caller, args, 1, out int amount)) {
						return;
					}

					progression.GrantXp(amount);
					caller.Reply($"Granted {amount} experience. Level {progression.Level}.", Info);
					break;

				case "level":
					if (!TryNumber(caller, args, 1, out int level)) {
						return;
					}

					level = Utils.Clamp(level, 1, XpCurve.MaxLevel);
					progression.UnspentPoints += (level - progression.Level) * XpCurve.PointsPerLevel;
					progression.Level = level;
					progression.Xp = 0;
					caller.Reply($"Level set to {level}.", Info);
					break;

				case "points":
					if (!TryNumber(caller, args, 1, out int points)) {
						return;
					}

					progression.UnspentPoints += points;
					caller.Reply($"Unspent points: {progression.UnspentPoints}.", Info);
					break;

				case "sigil":
					if (!TryNumber(caller, args, 1, out int sigilTier)) {
						return;
					}

					player.QuickSpawnItem(player.GetSource_Misc("RiftholdCommand"),
						RiftSigil.WithTier(sigilTier));
					caller.Reply($"Spawned a tier {sigilTier} Rift Sigil.", Info);
					break;

				case "enter":
					if (!TryNumber(caller, args, 1, out int enterTier)) {
						return;
					}

					if (!RiftGateway.TryEnter(player, enterTier, ignoreRequirements: true, out string reason)) {
						caller.Reply(reason, Problem);
						return;
					}

					break;

				case "exit":
					RiftGateway.ReturnToHub();
					break;

				case "hub":
					if (!TryNumber(caller, args, 1, out int stage)) {
						return;
					}

					ModContent.GetInstance<HubGrowthSystem>().SetStage(stage, announce: true);
					caller.Reply($"Hub growth stage set to {stage}.", Info);
					break;

				case "where":
					caller.Reply(RiftGateway.InRift
						? $"Inside a tier {RiftRunState.ActiveTier} rift."
						: "In the Hold.", Info);
					break;

				default:
					caller.Reply(Usage, Info);
					break;
			}
		}

		private static bool TryNumber(CommandCaller caller, string[] args, int index, out int value)
		{
			value = 0;
			if (args.Length <= index) {
				caller.Reply($"Missing number. {caller.CommandType}: /rift {args[0]} <number>", Problem);
				return false;
			}

			if (!int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
				caller.Reply($"'{args[index]}' is not a number.", Problem);
				return false;
			}

			return true;
		}
	}
}
