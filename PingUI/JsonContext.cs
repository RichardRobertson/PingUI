using System.Text.Json;
using System.Text.Json.Serialization;
using PingUI.Interop;
using PingUI.Models;
using PingUI.Services;

namespace PingUI;

/// <summary>
/// A helper object for serializing and deserializing JSON.
/// </summary>
[JsonSerializable(typeof(LocalOrAppDataJsonConfiguration))]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(WindowPlacement.WindowPlacementRecord))]
[JsonSerializable(typeof(Point.PointRecord))]
[JsonSerializable(typeof(Rect.RectRecord))]
internal partial class JsonContext : JsonSerializerContext
{
	/// <summary>
	/// A shared instance.
	/// </summary>
	public static JsonContext Instance
	{
		get;
	} = new(
		new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
		});
}
