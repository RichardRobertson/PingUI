using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PingUI.Tags;

/// <summary>
/// Represents a tag that matches a regular expression.
/// </summary>
/// <param name="Expression">The regular expression to match.</param>
public record RegExp(Regex Expression) : FilterBase
{
	/// <inheritdoc />
	public override bool IsMatch(IEnumerable<string> itemTags)
	{
		return itemTags.Any(Expression.IsMatch);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"/{Expression}/";
	}
}
