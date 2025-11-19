using System.Reflection;
using ECS.Components;
using ECS.Logs;
using ECS.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ECS.Prototypes;

[NeedDependencies]
public class PrototypeManager
{
    [SystemDependency] private readonly ComponentManager _compMan = default!;

    private List<Type> _prototypeTypes = new List<Type>();

    private List<IPrototype> _prototypes = new List<IPrototype>();

    /// <summary>
    /// A list of all loaded prototypes.
    /// </summary>
    public List<IPrototype> Prototypes { get => _prototypes; }

    /// <summary>
    /// Loads all the prototypes availible in relevant directory.
    /// </summary>
    /// <param name="verbouse"></param>
    public void LoadPrototypes(bool verbouse = false)
    {
        IPrototype LoadPrototype(string filePath)
        {
            // Read the file and parse the JSON
            string json = File.ReadAllText(filePath);
            var j = JObject.Parse(json);

            try
            {
                // Determine the type name from prototype
                string typeName = j["Type"]!.ToString();


                // Fill the prototype with data from JSON
                var finalType = _prototypeTypes.FirstOrDefault(pt => pt.Name == typeName && !pt.IsAbstract);
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new JSONComponentConverter(_compMan));
                var proto = j.ToObject(finalType, serializer)! as IPrototype;


                // Add the filled prototype back to the list
                _prototypes.Add(proto!);
                return proto!;
            }
            catch
            {
                throw new NullReferenceException("Required 'Type' property is unset");
            }
        }

        // Fill the prototypes with placeholders
        _prototypeTypes = Assembly.GetExecutingAssembly().GetTypes()
                                                .Where(t => typeof(IPrototype).IsAssignableFrom(t) && !t.IsAbstract)
                                                .ToList(); // Create instance and cast to IPrototype

        // The path to the prototypes folder
        string protoPath = Directory.GetCurrentDirectory() + "/Prototypes";
        var protoFiles = Directory.EnumerateFiles(protoPath, "*.json", SearchOption.AllDirectories);

        foreach (var file in protoFiles)
        {
            try
            {
                var proto = LoadPrototype(file);
                if (verbouse)
                    Logger.LogInfo($"Loaded prototype with ID '{proto.Id}' from file: {file}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load prototype from file {file}: {e.Message}");
            }
        }

    }

    /// <summary>
    /// Gets the loaded prototype by it's id.
    /// </summary>
    /// <typeparam name="TProto"></typeparam>
    /// <param name="id"></param>
    /// <returns>The loaded prototype.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
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

/// <summary>
/// Basic interface for all the prototypes.
/// </summary>
public interface IPrototype
{
    /// <summary>
    /// The unique ID of this prototype.
    /// </summary>
    public string Id { get; }
    // TODO: Implement this.
    public string Type { get; }
}