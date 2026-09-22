namespace Riftaria.Common.Net
{
	public enum RiftariaPacketType : byte
	{
		/// <summary>Full progression state for one player.</summary>
		PlayerProgression = 0,

		/// <summary>Server telling a client it earned experience.</summary>
		GrantExperience = 1,

		/// <summary>Client asking the server to spend a point. The server decides.</summary>
		RequestSpendPoint = 2,

		/// <summary>Client asking the server to open a rift. The server decides.</summary>
		RequestEnterRift = 3,

		/// <summary>Server broadcasting hub growth so every client agrees on the stage.</summary>
		HubStage = 4,

		/// <summary>Server telling everyone in a rift that it has been cleared.</summary>
		RunComplete = 5
	}
}
