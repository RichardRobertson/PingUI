using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PingUI.JsonConverters;

/// <summary>
/// Converts <see cref="IPAddress" /> values to and from JSON strings.
/// </summary>
public sealed class IPAddressJsonConverter : JsonConverter<IPAddress>
{
	/// <inheritdoc />
	public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.GetString() is string ipString)
		{
			return IPAddress.Parse(ipString);
		}
		return null;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
