using System.Collections.Generic;

namespace PingUI.Tags;

/// <summary>
/// Represents a negated tag match.
/// </summary>
/// <param name="Inner">The <see cref="Tag" /> to check and return the opposite match result for.</param>
public record Not(Tag Inner) : Tag
{
	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		return !Inner.IsMatch(itemTags);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		var inner = Inner.ToString();
		if (inner[0] == '(' && inner[^1] == ')')
		{
			return $"!{inner}";
		}
		else
		{
			return $"!({Inner})";
		}
	}
}
