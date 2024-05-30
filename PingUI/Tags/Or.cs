using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a collection of tags, of which at least one must match.
/// </summary>
public record Or : FilterBase
{
	/// <summary>
	/// Initializes a new <see cref="Or" />.
	/// </summary>
	/// <param name="tags">A set of tags to match.</param>
	public Or(IEnumerable<FilterBase> tags)
	{
		Tags = tags.SelectMany(tag => tag is Or or ? or.Tags : [tag]).ToImmutableArray();
	}

	/// <summary>
	/// Gets the set of tags, of which at least one must match.
	/// </summary>
	public ImmutableArray<FilterBase> Tags
	{
		get;
	}

	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		foreach (var tag in Tags)
		{
			if (tag.IsMatch(itemTags))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"({string.Join(" | ", Tags)})";
	}
}
