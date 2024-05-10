using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a collection of tags that must all match positively.
/// </summary>
public record And : Tag
{
	/// <summary>
	/// Initializes a new <see cref="And" />.
	/// </summary>
	/// <param name="tags">A set of tags to match.</param>
	public And(IEnumerable<Tag> tags)
	{
		Tags = tags.SelectMany(tag => tag is And and ? and.Tags : [tag]).ToImmutableArray();
	}

	/// <summary>
	/// Gets the set of all tags that must be matched.
	/// </summary>
	public ImmutableArray<Tag> Tags
	{
		get;
	}

	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		foreach (var tag in Tags)
		{
			if (!tag.IsMatch(itemTags))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"({string.Join(" & ", Tags)})";
	}
}
