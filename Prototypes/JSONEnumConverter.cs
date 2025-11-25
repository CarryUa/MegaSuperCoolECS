using ECS.System.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ECS.Prototypes;

public class JSONEnumConverter(EnumManager enumManager) : JsonConverter
{
    private EnumManager _enumManager = enumManager;

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsEnum;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
            throw new JsonSerializationException($"Failed to parse enum '{reader.Path}': Expected string value, but got {reader.TokenType}.");

        var raw = (string)reader.Value!;

        if (!raw.StartsWith("enum."))
            throw new JsonSerializationException($"Failed to parse enum '{reader.Path}': Enum value must begin with 'enum.', but got '{raw}'.");

        raw = raw.TrimStart("enum.".ToCharArray());

        var enumName = raw.Split(".")[0];
        var enumValue = raw.Split(".")[1];


        if (!_enumManager.TryGetEnumTypeByName(enumName, out var enumT))
            throw new JsonSerializationException($"Failed to parse enum '{reader.Path}': Enum doesn't exist");

        var enumObj = Enum.Parse(enumT!, enumValue);

        return enumObj;

    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

}