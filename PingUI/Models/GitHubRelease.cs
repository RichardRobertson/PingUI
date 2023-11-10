using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PingUI.Models;

/// <summary>
/// Represents the important pieces of a GitHub release JSON API response.
/// </summary>
public class GitHubRelease
{
	/// <summary>
	/// The tag name of the release.
	/// </summary>
	[JsonPropertyName("tag_name")]
	public string? TagName
	{
		get;
		set;
	}

	/// <summary>
	/// The Uri to open in the web browser.
	/// </summary>
	[JsonPropertyName("html_url")]
	public string? HtmlUrl
	{
		get;
		set;
	}

	/// <summary>
	/// The Markdown contents of the release.
	/// </summary>
	[JsonPropertyName("body")]
	public string? Body
	{
		get;
		set;
	}

	/// <summary>
	/// Convenience method for checking if all three properties are filled.
	/// </summary>
	[MemberNotNullWhen(true, nameof(TagName), nameof(HtmlUrl), nameof(Body))]
	public bool AllSet => TagName is not null && HtmlUrl is not null && Body is not null;
}
