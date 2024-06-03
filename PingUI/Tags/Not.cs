using System.Collections.Generic;

namespace PingUI.Tags;

/// <summary>
/// Represents a negated filter match.
/// </summary>
/// <param name="Inner">The <see cref="FilterBase" /> to check and return the opposite match result for.</param>
public record Not(FilterBase Inner) : FilterBase
{
	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		return !Inner.IsMatch(itemTags);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return Inner switch
		{
			// parenthesis for groups
			And _ or Or _ => $"!({Inner})",
			// cancel nested Not
			Not n => n.Inner.ToString(),
			// anything else; currently just Literal
			_ => $"!{Inner}",
		};
	}
}
