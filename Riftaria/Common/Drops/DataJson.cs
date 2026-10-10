using System.Text.Json;
using System.Text.Json.Serialization;

namespace Riftaria.Common.Drops
{
	/// <summary>
	/// The data files are JSON wrapped in a <c>var NAME = ...;</c> line, so the
	/// preview page can load them with a script tag.
	/// </summary>
	public static class DataJson
	{
		public static readonly JsonSerializerOptions Options = new() {
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
		};

		public static T Read<T>(string text) where T : class, new()
		{
			if (string.IsNullOrWhiteSpace(text)) {
				return new T();
			}

			int start = text.IndexOf('{');
			int end = text.LastIndexOf('}');
			if (start < 0 || end <= start) {
				return new T();
			}

			return JsonSerializer.Deserialize<T>(text.Substring(start, end - start + 1), Options) ?? new T();
		}

		public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
	}
}
