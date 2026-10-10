using System.Text;
using System.Text.Json;
using Riftaria.Common.Drops;
using RollServer;

// The preview page's roller. server.js starts this once and keeps it running:
// one JSON request per line on stdin, one JSON reply per line on stdout. Every
// roll goes through the mod's own ItemRoller and RiftLootPlan, so the page can
// never disagree with the game.

var random = new Random();
Func<int, int> next = random.Next;
using var input = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false));
using var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };

string line;
while ((line = input.ReadLine()) != null) {
	if (string.IsNullOrWhiteSpace(line)) {
		continue;
	}

	var reply = new Reply();
	try {
		using JsonDocument doc = JsonDocument.Parse(line);
		JsonElement request = doc.RootElement;
		if (request.TryGetProperty("id", out JsonElement id) && id.ValueKind == JsonValueKind.Number) {
			reply.Id = id.GetInt32();
		}

		reply.Result = Rolls.Handle(request, next);
		reply.Ok = true;
	}
	catch (Exception error) {
		reply.Error = error.Message;
	}

	output.WriteLine(JsonSerializer.Serialize(reply, DataJson.Options));
}

namespace RollServer
{
	internal sealed class Reply
	{
		public int Id { get; set; }

		public bool Ok { get; set; }

		public object Result { get; set; }

		public string Error { get; set; }
	}
}
