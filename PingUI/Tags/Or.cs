using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Represents a collection of filters, of which at least one must match.
/// </summary>
public record Or : FilterBase
{
	/// <summary>
	/// Initializes a new <see cref="Or" />.
	/// </summary>
	/// <param name="filters">A set of filters to match.</param>
	public Or(IEnumerable<FilterBase> filters)
	{
		Filters = filters.SelectMany(filter => filter is Or or ? or.Filters : [filter]).ToImmutableArray();
	}

	/// <summary>
	/// Gets the set of filters, of which at least one must match.
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
			if (filter.IsMatch(itemTags))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return string.Join(" | ", Filters);
	}
}
