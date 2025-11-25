using System.Reflection;

namespace ECS.System.Enums;

[NeedDependencies]
public class EnumManager
{
    public List<Type> EnumTypes { get => _enumTypes; }
    private List<Type> _enumTypes = [];

    public void InitializeEnums()
    {
        var asm = Assembly.GetExecutingAssembly();

        foreach (var t in asm.GetTypes())
        {
            if (!t.IsEnum) continue;
            _enumTypes.Add(t);
        }
    }

    public bool TryGetEnumTypeByName(string enumName, out Type? enumType)
    {
        foreach (var t in _enumTypes)
        {
            if (t.Name == enumName)
            {
                enumType = t;
                return true;
            }
        }
        enumType = null;
        return false;
    }
}