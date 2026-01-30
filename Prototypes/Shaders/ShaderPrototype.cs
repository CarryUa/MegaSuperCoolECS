using ECS.Logs;
using ECS.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace ECS.Prototypes.Shaders;

public class ShaderPrototype : IPrototype, IShader
{
    #region From Proto
    public string Id { get; set; } = "";

    /// <summary>
    /// Path to the shader .hlsl file.
    /// </summary>
    public string ShaderPath = "";

    /// <summary>
    /// The type of the shader from OpenGL4 enum.
    /// </summary>
    public ShaderType ShaderType;

    /// <summary>
    /// The dictionary of uniforms and their types.
    /// </summary>
    public Dictionary<string, UniformType> Uniforms = [];

    /// <summary>
    /// The dictionary of attributes and their types.
    /// </summary>
    public Dictionary<string, AttributeType> Attributes = [];

    #endregion

    #region From system

    /// <summary>
    /// The 
    /// </summary>
    public int ShaderID { get; set; }
    #endregion
}