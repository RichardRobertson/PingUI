using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PingUI.Models;

public record AutomaticTagEntry
{
	private readonly Regex? regex;

	public AutomaticTagEntry(AutomaticTagSource source, AutomaticTagType type, string content, string tag)
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
				AutomaticTagType.Contains,
				AutomaticTagType.IsExactly,
				AutomaticTagType.StartsWith,
				AutomaticTagType.EndsWith,
				AutomaticTagType.MatchesRegex,
				AutomaticTagType.DoesNotContain,
				AutomaticTagType.DoesNotMatchExactly,
				AutomaticTagType.DoesNotStartWith,
				AutomaticTagType.DoesNotEndWith,
				AutomaticTagType.DoesNotMatchRegex,
			}
			.Contains(type))
		{
			throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(AutomaticTagType));
		}
		ArgumentException.ThrowIfNullOrWhiteSpace(content);
		ArgumentException.ThrowIfNullOrWhiteSpace(tag);
		if (new[]
			{
				AutomaticTagType.MatchesRegex,
				AutomaticTagType.DoesNotMatchRegex,
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

	public AutomaticTagType Type
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
			AutomaticTagType.Contains => source.Contains(Content),
			AutomaticTagType.IsExactly => source.Equals(Content),
			AutomaticTagType.StartsWith => source.StartsWith(Content),
			AutomaticTagType.EndsWith => source.EndsWith(Content),
			AutomaticTagType.MatchesRegex => regex!.IsMatch(source),
			AutomaticTagType.DoesNotContain => !source.Contains(Content),
			AutomaticTagType.DoesNotMatchExactly => !source.Equals(Content),
			AutomaticTagType.DoesNotStartWith => !source.StartsWith(Content),
			AutomaticTagType.DoesNotEndWith => !source.EndsWith(Content),
			AutomaticTagType.DoesNotMatchRegex => !regex!.IsMatch(source),
			_ => throw new UnreachableException(),
		};
	}
}
