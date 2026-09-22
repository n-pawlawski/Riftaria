using System.Globalization;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;

namespace Riftaria.Common.Commands
{
	/// <summary>
	/// Developer shortcuts. Every one of these exists because the alternative is
	/// replaying the loop from tier one to check a change three tiers deep.
	/// </summary>
	public class RiftCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "rift";

		public override string Usage =>
			"/rift enter <tier> | exit | clear <tier> | reset | tiers | where";

		public override string Description => "Riftaria developer commands.";

		private static readonly Color Info = new(150, 200, 255);
		private static readonly Color Problem = new(255, 140, 140);

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length == 0) {
				caller.Reply(Usage, Info);
				return;
			}

			Player player = caller.Player;
			HubWorldSystem hub = HubWorldSystem.Instance;

			switch (args[0].ToLowerInvariant()) {
				case "enter":
					if (!TryNumber(caller, args, 1, out int enterTier)) {
						return;
					}

					if (!RiftGateway.TryEnter(player, enterTier, ignoreRequirements: true, out string reason)) {
						caller.Reply(reason, Problem);
					}

					break;

				case "exit":
					RiftGateway.ReturnToHub();
					break;

				case "clear":
					if (!TryNumber(caller, args, 1, out int clearTier)) {
						return;
					}

					hub.RecordClear(clearTier);
					caller.Reply($"Highest tier cleared is now {hub.HighestTierCleared}. "
						+ $"Tier {RiftGateway.HighestUnlockedTier} is open.", Info);
					break;

				case "reset":
					hub.HighestTierCleared = 0;
					caller.Reply("Progress reset. Only tier 1 is open.", Info);
					break;

				case "tiers":
					caller.Reply($"{RiftTierRegistry.MaxTier} tiers registered. "
						+ $"Cleared {hub.HighestTierCleared}, open up to {RiftGateway.HighestUnlockedTier}.", Info);
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
				caller.Reply($"Missing number. Usage: /rift {args[0]} <number>", Problem);
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
