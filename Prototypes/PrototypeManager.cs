using System.Reflection;
using ECS.Logs;
using ECS.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ECS.Prototypes;

[NeedDependencies]
public class PrototypeManager
{
    private List<IPrototype> _prototypes = new List<IPrototype>();

    /// <summary>
    /// A list of all loaded prototypes.
    /// </summary>
    public List<IPrototype> Prototypes => _prototypes;

    public void LoadPrototypes(bool verbouse = false)
    {
        void LoadPrototype(string filePath)
        {
            // Read the file and parse the JSON
            string json = File.ReadAllText(filePath);
            var j = JObject.Parse(json);

            // Determine the type name from prototype
            string typeName = j["Type"]!.ToString();

            // Find the matching prototype type and remove if from list
            var protoType = _prototypes.FirstOrDefault(p => p.GetType().Name == typeName && !p.GetType().IsAbstract);
            _prototypes.Remove(protoType!);

            // Fill the prototype with data from JSON
            var finalType = protoType!.GetType();
            protoType = j.ToObject(finalType)! as IPrototype;

            // Add the filled prototype back to the list
            _prototypes.Add(protoType!);
        }

        // Fill the prototypes with placeholders
        _prototypes = Assembly.GetExecutingAssembly().GetTypes()
                                                .Where(t => typeof(IPrototype).IsAssignableFrom(t) && !t.IsAbstract) // Is implementing IPrototype and not abstract
                                                .Select(t => (IPrototype)Activator.CreateInstance(t)!).ToList(); // Create instance and cast to IPrototype

        // The path to the prototypes folder
        string protoPath = Directory.GetCurrentDirectory() + "/Prototypes";
        var protoFiles = Directory.EnumerateFiles(protoPath, "*.json");

        foreach (var file in protoFiles)
        {
            try
            {
                LoadPrototype(file);
                if (verbouse)
                    Logger.LogInfo($"Loaded prototype from file: {file}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load prototype from file {file}: {e.Message}");
            }
        }

    }

    public TProto GetPrototype<TProto>(string id)
    where TProto : IPrototype
    {
        var proto = _prototypes.FirstOrDefault(p => p.Id == id && p is TProto);
        if (proto == null)
        {
            throw new KeyNotFoundException($"Prototype with ID '{id}' not found.");
        }
        return (TProto)proto;
    }
}

public interface IPrototype
{
    /// <summary>
    /// The unique ID of this prototype.
    /// </summary>
    public string Id { get; }
    public string Type { get; }
}

public class Prototype : IPrototype
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";

    public int TestField = 0;
}

public class AnotherPrototype : IPrototype
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";

    public string AnotherField = "Hello World";
}