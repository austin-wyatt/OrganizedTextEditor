using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrganizedTextEditor.Classes
{
	public static class Serializer
	{
		private static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters = { new FlowDocumentJsonConverter(), new IdJsonConverter() },
			IncludeFields = true
		};

		public static string Serialize<T>(T obj)
		{
			return JsonSerializer.Serialize(obj, Options);
		}

		public static T? Deserialize<T>(string json)
		{
			try
			{
				return JsonSerializer.Deserialize<T>(json, Options);
			}
			catch
			{
				return default;
			}
		}
	}

	public class FlowDocumentJsonConverter : JsonConverter<FlowDocument>
	{
		public override FlowDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new FlowDocument(new Paragraph(new Run(reader.GetString())));
		}

		public override void Write(Utf8JsonWriter writer, FlowDocument value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(new TextRange(value.ContentStart, value.ContentEnd).Text);
		}
	}

	public class IdJsonConverter : JsonConverter<Id>
	{
		public override Id Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ReadOnlySpan<char> byteString = reader.GetString().AsSpan();

			byte[] bytes = new byte[byteString.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = byte.Parse(byteString.Slice(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
			}

			return Id.FromByteArray(bytes);
		}

		public override void Write(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToHexString());
		}
	}
}
