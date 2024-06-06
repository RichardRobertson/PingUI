using System;
using System.Collections.Generic;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a tag that matches a literal string.
/// </summary>
/// <param name="Content">The literal string to match.</param>
public record Literal(string Content) : FilterBase
{
	protected static readonly System.Buffers.SearchValues<char> TokenSeparatorChars = System.Buffers.SearchValues.Create(" \t()&|!\"\'`");

	/// <inheritdoc />
	public override bool IsMatch(IEnumerable<string> itemTags)
	{
		return itemTags.Contains(Content);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		if (Content.AsSpan().IndexOfAny(TokenSeparatorChars) != -1)
		{
			var quote = Content.Contains('\"');
			var apostrophe = Content.Contains('\'');
			var backtick = Content.Contains('`');
			if (!quote)
			{
				return $"\"{Content}\"";
			}
			else if (!apostrophe)
			{
				return $"\'{Content}\'";
			}
			else if (!backtick)
			{
				return $"`{Content}`";
			}
			else
			{
				throw new FormatException("Cannot display a Literal that contains all three of \" (quote) \' (apostrophe) ` (backtick)");
			}
		}
		return Content;
	}
}
