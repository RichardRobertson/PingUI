using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a collection of filters that must all match positively.
/// </summary>
public record And : FilterBase
{
	/// <summary>
	/// Initializes a new <see cref="And" />.
	/// </summary>
	/// <param name="filters">A set of filters to match.</param>
	public And(IEnumerable<FilterBase> filters)
	{
		Filters = filters.SelectMany(filter => filter is And and ? and.Filters : [filter]).ToImmutableArray();
	}

	/// <summary>
	/// Gets the set of all filters that must be matched.
	/// </summary>
	public ImmutableArray<FilterBase> Filters
	{
		get;
	}

	/// <inheritdoc />
	public override bool IsMatch(IReadOnlyList<string> itemTags)
	{
		foreach (var filter in Filters)
		{
			if (!filter.IsMatch(itemTags))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return string.Join(" & ", Filters.Select(filter => filter is Or ? $"({filter})" : filter.ToString()));
	}
}
