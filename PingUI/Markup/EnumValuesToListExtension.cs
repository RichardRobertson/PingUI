using System;
using Avalonia.Markup.Xaml;

namespace PingUI.Markup;

public class EnumValuesToListExtension<TEnum> : MarkupExtension where TEnum : struct, Enum
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return Enum.GetValues<TEnum>();
	}
}
