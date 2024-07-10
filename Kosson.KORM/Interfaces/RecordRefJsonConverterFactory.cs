using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Kosson.KORM;

/// <summary>
/// Converts RecordRefs to and from JSON.
/// </summary>
public class RecordRefJsonConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType) return false;
		if (typeToConvert.GetGenericTypeDefinition() != typeof(RecordRef<>)) return false;
		return true;
	}

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		return (JsonConverter)Activator.CreateInstance(typeof(RecordRefJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;
	}

	private class RecordRefJsonConverter<TRecord> : JsonConverter<RecordRef<TRecord>>
		where TRecord : IRecord
	{
		public override RecordRef<TRecord> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new RecordRef<TRecord>(reader.GetInt64());
		}

		public override void Write(Utf8JsonWriter writer, RecordRef<TRecord> value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value.ID);
		}
	}
}