using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>A room the catalog can roll, and how often relative to the others.</summary>
	public readonly struct RiftRoomEntry
	{
		public int Weight { get; }

		public Func<Random, RiftPiece> Make { get; }

		public RiftRoomEntry(int weight, Func<Random, RiftPiece> make)
		{
			Weight = weight;
			Make = make;
		}
	}

	/// <summary>
	/// The pieces one tier builds from, and how long its paths run. Pieces are
	/// made fresh for each placement, so a room can roll its own sockets.
	/// </summary>
	public sealed class RiftCatalog
	{
		public Func<Random, RiftPiece> Start { get; init; }

		public Func<Random, RiftPiece> Boss { get; init; }

		public Func<Random, RiftPiece> DeadEnd { get; init; }

		public IReadOnlyList<RiftRoomEntry> Rooms { get; init; }

		/// <summary>Rooms between the start and the boss.</summary>
		public int MainMin { get; init; } = 3;

		public int MainMax { get; init; } = 5;

		public int BranchMin { get; init; } = 1;

		public int BranchMax { get; init; } = 3;

		/// <summary>Rooms in a branch before its dead end.</summary>
		public int BranchRoomsMax { get; init; } = 2;

		/// <summary>Enemy kind placed in the corridor into the boss. Negative for none.</summary>
		public int ApproachMark { get; init; } = -1;

		/// <summary>Direction the main path prefers, in degrees, and how strongly.</summary>
		public double LeanAngle { get; init; }

		public double LeanStrength { get; init; } = 1.0;

		/// <summary>Steepest connector run allowed when <see cref="Shafts"/> is off.</summary>
		public double MaxGrade { get; init; } = RiftConnector.MaxGrade;

		/// <summary>Whether connectors may be steeper than walkable and get platform rungs.</summary>
		public bool Shafts { get; init; } = true;

		/// <summary>Level stretch at each end of a connector, straight out of the socket.</summary>
		public int LandingMin { get; init; } = 4;

		public int LandingMax { get; init; } = 12;

		/// <summary>Length of the free run between the two landings.</summary>
		public int SpanMin { get; init; } = 8;

		public int SpanMax { get; init; } = 30;

		/// <summary>
		/// How far the free run tilts from the line between the landings, in
		/// degrees. Positive tilts down, whichever way the run heads.
		/// </summary>
		public int DipMin { get; init; } = -30;

		public int DipMax { get; init; } = 30;

		public RiftPiece Room(Random rand)
		{
			int total = 0;
			foreach (RiftRoomEntry entry in Rooms) {
				total += entry.Weight;
			}

			int roll = rand.Next(total);
			foreach (RiftRoomEntry entry in Rooms) {
				roll -= entry.Weight;
				if (roll < 0) {
					return entry.Make(rand);
				}
			}

			return Rooms[Rooms.Count - 1].Make(rand);
		}
	}
}
