using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.NetworkInformation;
using Avalonia.Data.Converters;
using PingUI.I18N;
using PingUI.Models;

namespace PingUI.Converters;

public class EnumLocalizerConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value switch
		{
			AutomaticTagSource.Label => Strings.PingUI_Models_AutomaticTagSource_Label,
			AutomaticTagSource.Address => Strings.PingUI_Models_AutomaticTagSource_Address,
			MatchType.Contains => Strings.PingUI_Models_MatchType_Contains,
			MatchType.IsExactly => Strings.PingUI_Models_MatchType_IsExactly,
			MatchType.StartsWith => Strings.PingUI_Models_MatchType_StartsWith,
			MatchType.EndsWith => Strings.PingUI_Models_MatchType_EndsWith,
			MatchType.MatchesRegex => Strings.PingUI_Models_MatchType_MatchesRegex,
			MatchType.DoesNotContain => Strings.PingUI_Models_MatchType_DoesNotContain,
			MatchType.DoesNotMatchExactly => Strings.PingUI_Models_MatchType_DoesNotMatchExactly,
			MatchType.DoesNotStartWith => Strings.PingUI_Models_MatchType_DoesNotStartWith,
			MatchType.DoesNotEndWith => Strings.PingUI_Models_MatchType_DoesNotEndWith,
			MatchType.DoesNotMatchRegex => Strings.PingUI_Models_MatchType_DoesNotMatchRegex,
			IPStatus.Success => Strings.System_Net_NetworkInformation_IPStatus_Success,
			IPStatus.DestinationNetworkUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationNetworkUnreachable,
			IPStatus.DestinationHostUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationHostUnreachable,
			// DestinationProhibited and DestinationProtocolUnreachable have the same value
			// DestinationProtocolUnreachable is for IPv4 and DestinationProhibited is for IPv6
			// IPStatus.DestinationProhibited => Strings.System_Net_NetworkInformation_IPStatus_DestinationProhibited,
			IPStatus.DestinationProtocolUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationProtocolUnreachable,
			IPStatus.DestinationPortUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationPortUnreachable,
			IPStatus.NoResources => Strings.System_Net_NetworkInformation_IPStatus_NoResources,
			IPStatus.BadOption => Strings.System_Net_NetworkInformation_IPStatus_BadOption,
			IPStatus.HardwareError => Strings.System_Net_NetworkInformation_IPStatus_HardwareError,
			IPStatus.PacketTooBig => Strings.System_Net_NetworkInformation_IPStatus_PacketTooBig,
			IPStatus.TimedOut => Strings.System_Net_NetworkInformation_IPStatus_TimedOut,
			IPStatus.BadRoute => Strings.System_Net_NetworkInformation_IPStatus_BadRoute,
			IPStatus.TtlExpired => Strings.System_Net_NetworkInformation_IPStatus_TtlExpired,
			IPStatus.TtlReassemblyTimeExceeded => Strings.System_Net_NetworkInformation_IPStatus_TtlReassemblyTimeExceeded,
			IPStatus.ParameterProblem => Strings.System_Net_NetworkInformation_IPStatus_ParameterProblem,
			IPStatus.SourceQuench => Strings.System_Net_NetworkInformation_IPStatus_SourceQuench,
			IPStatus.BadDestination => Strings.System_Net_NetworkInformation_IPStatus_BadDestination,
			IPStatus.DestinationUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationUnreachable,
			IPStatus.TimeExceeded => Strings.System_Net_NetworkInformation_IPStatus_TimeExceeded,
			IPStatus.BadHeader => Strings.System_Net_NetworkInformation_IPStatus_BadHeader,
			IPStatus.UnrecognizedNextHeader => Strings.System_Net_NetworkInformation_IPStatus_UnrecognizedNextHeader,
			IPStatus.IcmpError => Strings.System_Net_NetworkInformation_IPStatus_IcmpError,
			IPStatus.DestinationScopeMismatch => Strings.System_Net_NetworkInformation_IPStatus_DestinationScopeMismatch,
			IPStatus.Unknown => Strings.System_Net_NetworkInformation_IPStatus_Unknown,
			_ => throw new InvalidEnumArgumentException("Value provided could not be localized."),
		};
	}

	[SuppressMessage("", "RCS1079", Justification = "No need to convert localized display value back.")]
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
