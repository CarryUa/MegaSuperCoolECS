
using ECS.Components;
using ECS.Logs;
using ECS.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ECS.Prototypes;

public class JSONComponentConverter : JsonConverter
{
    private readonly ComponentManager _compManager;

    public JSONComponentConverter(ComponentManager compManager)
    {
        _compManager = compManager;
    }
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IComponent).IsAssignableFrom(typeToConvert);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Get the type field from component
        var jObject = JObject.Load(reader);
        var typeName = jObject["Type"]?.ToString();

        // Ensure type is specified
        if (typeName is null)
            throw new JsonSerializationException("Missing required Type property in component JSON.");

        // Get the actual type
        if (!_compManager.TryGetComponentType(typeName, out var type))
            Logger.LogFatal(new JsonSerializationException($"Failed to get component type from name: \"{typeName}\""));

        serializer.Converters.Remove(this);
        // return the object
        return jObject.ToObject(type, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}