namespace ECS.System;

#region Attribute Def

/// <summary>
/// Marks field as dependency to be injected during initializing.        
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SystemDependency : Attribute
{
}

/// <summary>
/// Used for Systems and other classes that require dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NeedDependencies : Attribute
{
}

/// <summary>
/// Used for Systems and other classes that require initialization. Defines Priority of initialization. Classes without Priority will be initialized last.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class InitializationPriority(InitPriority Priority) : Attribute
{
    public InitPriority Priority { get; } = Priority;
}
#endregion