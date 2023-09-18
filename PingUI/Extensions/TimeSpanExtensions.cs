using System;
using System.Text;
using PingUI.I18N;

namespace PingUI.Extensions;

/// <summary>
/// Extensions to <see cref="TimeSpan" />.
/// </summary>
public static class TimeSpanExtensions
{
	/// <summary>
	/// Converts a <see cref="TimeSpan" /> to words in the form of "x hours, y minutes, z seconds" with correct plurals and dropped segments for 0 values.
	/// </summary>
	/// <param name="this">The <see cref="TimeSpan" /> to convert.</param>
	/// <returns>A <see cref="string" /> representing the given <see cref="TimeSpan" /> in words.</returns>
	public static string ToWords(this TimeSpan @this)
	{
		var written = false;
		var result = new StringBuilder();
		if (@this.Hours != 0)
		{
			result.Append(@this.Hours);
			result.Append(' ');
			if (@this.Hours == 1)
			{
				result.Append(Strings.TimeSpanExtensions_Hour);
			}
			else
			{
				result.Append(Strings.TimeSpanExtensions_Hours);
			}
			written = true;
		}
		if (@this.Minutes != 0)
		{
			if (written)
			{
				result.Append(Strings.TimeSpanExtensions_Separator);
			}
			result.Append(@this.Minutes);
			result.Append(' ');
			if (@this.Minutes == 1)
			{
				result.Append(Strings.TimeSpanExtensions_Minute);
			}
			else
			{
				result.Append(Strings.TimeSpanExtensions_Minutes);
			}
			written = true;
		}
		if (@this.Seconds != 0)
		{
			if (written)
			{
				result.Append(Strings.TimeSpanExtensions_Separator);
			}
			result.Append(@this.Seconds);
			result.Append(' ');
			if (@this.Seconds == 1)
			{
				result.Append(Strings.TimeSpanExtensions_Second);
			}
			else
			{
				result.Append(Strings.TimeSpanExtensions_Seconds);
			}
		}
		return result.ToString();
	}
}
