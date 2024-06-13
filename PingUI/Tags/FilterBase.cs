using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PingUI.Tags;

/// <summary>
/// Base class for tag matching system.
/// </summary>
public abstract record FilterBase : IParsable<FilterBase>
{
	private static readonly System.Buffers.SearchValues<char> TokenSeparatorChars = System.Buffers.SearchValues.Create(" \t()&|!");

	/// <inheritdoc />
	public static FilterBase Parse(string s, IFormatProvider? provider)
	{
		if (TryParseInner(s, out var result, out var exception))
		{
			return result;
		}
		else
		{
			throw exception;
		}
	}

	/// <inheritdoc />
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FilterBase result)
	{
		if (s is null)
		{
			result = null;
			return false;
		}
		return TryParseInner(s, out result, out var _);
	}

	/// <inheritdoc cref="TryParse" />
	/// <param name="exception">The <see cref="Exception" /> to be throw if <see cref="TryParseInner" /> returns <see langword="false" />.</param>
	private static bool TryParseInner(string s, [MaybeNullWhen(false)] out FilterBase result, [NotNullWhen(false)] out Exception? exception)
	{
		/*
			grammar = orExpression;

			orExpression = andExpression, {{whitespace}, "|", {whitespace}, andExpression};
			andExpression = primary, {{whitespace}, "&", {whitespace}, primary};
			primary = "(", {whitespace}, orExpression, {whitespace}, ")"
				| "!", {whitespace}, primary
				| literal;
			literal = anyNonSeparatorCharacter, {anyNonSeparatorCharacter}
				| '"', anyNonQuoteCharacter, {anyNonQuoteCharacter}, '"'
				| "'", anyNonApostropheCharacter, {anyNonApostropheCharacter}, "'"
				| "`", anyNonBacktickCharacter, {anyNonBacktickCharacter}, "`"
				| "/", anyNonOrEscapedSlashCharacter, {anyNonOrEscapedSlashCharacter}, "/";
		*/

		static bool TryReadPrimary(ref ReadOnlySpan<char> span, [NotNullWhen(true)] out FilterBase? result, [NotNullWhen(false)] out Exception? exception)
		{
			result = null;
			exception = null;
			if (span.IsEmpty)
			{
				exception = new FormatException("Unexpected end of input");
				return false;
			}
			else if (span[0] == '(')
			{
				span = span[1..];
				if (!TryReadOr(ref span, out result, out exception))
				{
					return false;
				}
				span = span.TrimStart();
				if (span.IsEmpty || span[0] != ')')
				{
					exception = new FormatException("Missing end parenthesis");
					return false;
				}
				span = span[1..];
				return true;
			}
			else if (span[0] == '!')
			{
				span = span[1..].TrimStart();
				if (!TryReadPrimary(ref span, out result, out exception))
				{
					exception = new FormatException("Operand missing after `!`");
					return false;
				}
				result = new Not(result);
				return true;
			}
			else if (span[0] == '\"' || span[0] == '\'' || span[0] == '`')
			{
				var nextQuote = span[1..].IndexOf(span[0]) + 1;
				if (nextQuote == 0)
				{
					result = null;
					exception = new FormatException("Missing end quote");
					return false;
				}
				result = new Literal(new string(span[1..nextQuote]));
				span = span[(nextQuote + 1)..];
			}
			else if (span[0] == '/')
			{
				var nextSlash = -1;
				var escaped = false;
				for (var i = 1; i < span.Length; i++)
				{
					if (span[i] == '\\')
					{
						escaped = !escaped;
					}
					else if (span[i] == '/' && !escaped)
					{
						nextSlash = i;
						break;
					}
					else if (escaped)
					{
						escaped = false;
					}
				}
				if (nextSlash == -1)
				{
					result = null;
					exception = new FormatException("Missing end slash");
					return false;
				}
				try
				{
					result = new RegExp(new Regex(new string(span[1..nextSlash])));
					span = span[(nextSlash + 1)..];
				}
				catch (Exception ex)
				{
					exception = ex;
					return false;
				}
			}
			else
			{
				var nextSeparator = span.IndexOfAny(TokenSeparatorChars);
				if (nextSeparator == -1)
				{
					result = new Literal(new string(span));
					span = span[span.Length..];
				}
				else
				{
					result = new Literal(new string(span[..nextSeparator]));
					span = span[nextSeparator..];
				}
			}
			return true;
		}

		static bool TryReadAnd(ref ReadOnlySpan<char> span, [NotNullWhen(true)] out FilterBase? result, [NotNullWhen(false)] out Exception? exception)
		{
			result = null;
			if (!TryReadPrimary(ref span, out var filter, out exception))
			{
				return false;
			}
			var list = new List<FilterBase>()
			{
				filter
			};
			while (!span.IsEmpty)
			{
				span = span.TrimStart();
				if (span.IsEmpty || span[0] != '&')
				{
					break;
				}
				span = span[1..].TrimStart();
				if (!TryReadPrimary(ref span, out filter, out exception))
				{
					exception = new FormatException("Operand missing after `&`", exception);
					return false;
				}
				list.Add(filter);
			}
			result = list.Count == 1 ? list[0] : new And(list);
			return true;
		}

		static bool TryReadOr(ref ReadOnlySpan<char> span, [NotNullWhen(true)] out FilterBase? result, [NotNullWhen(false)] out Exception? exception)
		{
			result = null;
			if (!TryReadAnd(ref span, out var filter, out exception))
			{
				return false;
			}
			var list = new List<FilterBase>()
			{
				filter
			};
			while (!span.IsEmpty)
			{
				span = span.TrimStart();
				if (span.IsEmpty || span[0] != '|')
				{
					break;
				}
				span = span[1..].TrimStart();
				if (!TryReadAnd(ref span, out filter, out exception))
				{
					exception = new FormatException("Operand missing after `|`", exception);
					return false;
				}
				list.Add(filter);
			}
			result = list.Count == 1 ? list[0] : new Or(list);
			return true;
		}

		if (s is null)
		{
			exception = new ArgumentNullException(nameof(s));
			result = null;
			return false;
		}
		var span = s.AsSpan().Trim();
		return TryReadOr(ref span, out result, out exception);
	}

	/// <summary>
	/// Checks to see if an item matches by its tag list.
	/// </summary>
	/// <param name="itemTags">A set of strings that represents all tags of the item to match.</param>
	/// <returns><see langword="true" /> if <paramref name="itemTags" /> matches this <see cref="FilterBase" />; otherwise <see langword="false" />.</returns>
	public abstract bool IsMatch(IEnumerable<string> itemTags);
}
