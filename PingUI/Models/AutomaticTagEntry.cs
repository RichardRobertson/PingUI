using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PingUI.Models;

public record AutomaticTagEntry
{
	private readonly Regex? regex;

	public AutomaticTagEntry(AutomaticTagSource source, MatchType type, string content, string tag)
	{
		if (!new[]
			{
				AutomaticTagSource.Label,
				AutomaticTagSource.Address,
			}
			.Contains(source))
		{
			throw new InvalidEnumArgumentException(nameof(source), (int)source, typeof(AutomaticTagSource));
		}
		if (!new[]
			{
				MatchType.Contains,
				MatchType.IsExactly,
				MatchType.StartsWith,
				MatchType.EndsWith,
				MatchType.MatchesRegex,
				MatchType.DoesNotContain,
				MatchType.DoesNotMatchExactly,
				MatchType.DoesNotStartWith,
				MatchType.DoesNotEndWith,
				MatchType.DoesNotMatchRegex,
			}
			.Contains(type))
		{
			throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(MatchType));
		}
		ArgumentException.ThrowIfNullOrWhiteSpace(content);
		ArgumentException.ThrowIfNullOrWhiteSpace(tag);
		if (new[]
			{
				MatchType.MatchesRegex,
				MatchType.DoesNotMatchRegex,
			}
			.Contains(type))
		{
			regex = new Regex(content);
		}
		Source = source;
		Type = type;
		Content = content;
		Tag = tag;
	}

	public AutomaticTagSource Source
	{
		get;
	}

	public MatchType Type
	{
		get;
	}

	public string Content
	{
		get;
	}

	public string Tag
	{
		get;
	}

	public bool Matches(Target target)
	{
		var source = Source switch
		{
			AutomaticTagSource.Label => target.Label ?? string.Empty,
			AutomaticTagSource.Address => target.Address.ToString(),
			_ => throw new UnreachableException(),
		};
		return Type switch
		{
			MatchType.Contains => source.Contains(Content),
			MatchType.IsExactly => source.Equals(Content),
			MatchType.StartsWith => source.StartsWith(Content),
			MatchType.EndsWith => source.EndsWith(Content),
			MatchType.MatchesRegex => regex!.IsMatch(source),
			MatchType.DoesNotContain => !source.Contains(Content),
			MatchType.DoesNotMatchExactly => !source.Equals(Content),
			MatchType.DoesNotStartWith => !source.StartsWith(Content),
			MatchType.DoesNotEndWith => !source.EndsWith(Content),
			MatchType.DoesNotMatchRegex => !regex!.IsMatch(source),
			_ => throw new UnreachableException(),
		};
	}
}
