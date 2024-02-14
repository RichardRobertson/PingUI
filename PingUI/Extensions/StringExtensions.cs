using System.Diagnostics.CodeAnalysis;

namespace PingUI.Extensions;

public static class StringExtensions
{
	[return: NotNullIfNotNull(nameof(@this))]
	public static string? ToCamel(this string? @this)
	{
		if (@this is null)
		{
			return null;
		}
		else if (@this.Length == 0)
		{
			return @this;
		}
		else
		{
			return char.ToLower(@this[0]) + @this[1..];
		}
	}
}
