using System.Collections.Generic;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a tag that matches a literal string.
/// </summary>
/// <param name="Content">The literal string to match.</param>
public record Literal(string Content) : FilterBase
{
	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		return itemTags.Contains(Content);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		if (Content.Contains('\"'))
		{
			return $"\'{Content}\'";
		}
		else
		{
			return $"\"{Content}\"";
		}
	}
}
