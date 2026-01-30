using ECS.Logs;
using OpenTK.Graphics.OpenGL4;

namespace ECS.Prototypes.Shaders;

public class ShaderProgramPrototype : IPrototype, IDisposable
{
    public string Id { get; set; } = "";

    /// <summary>
    /// The protoID of vertex shader;
    /// </summary>
    public required string VertexShader;

    /// <summary>
    /// The protoID of fragment shader;
    /// </summary>
    public required string FragmentShader;

    #region From System

    public int Handle;

    private bool disposed = false;

    public Dictionary<string, (UniformType, int)> UniformLocations = [];
    public Dictionary<string, (AttributeType, int)> AttributeLocations = [];


    public void Dispose()
    {
        if (!disposed)
        {
            GL.DeleteProgram(Handle);
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public bool TrySetUniform(string name, UniformType type, object value)
    {
        if (!UniformLocations.TryGetValue(name, out var uniformInfo))
        {
            Logger.LogError($"Couldn't find uniform '{name}' in shader program {this}");
            return false;
        }
        if (uniformInfo.Item1 != type)
        {
            Logger.LogError($"Type mismatch between uniform '{name}' in shader program {this}. Expected {uniformInfo.Item1} but got {type}");
            return false;
        }
        int location = uniformInfo.Item2;
        switch (type)
        {
            case UniformType.Float:
                GL.Uniform1(location, (float)value);
                break;
            case UniformType.Int:
                GL.Uniform1(location, (int)value);
                break;
            case UniformType.UnsignedInt:
                GL.Uniform1(location, (uint)value);
                break;
            case UniformType.FloatVec2:
                GL.Uniform2(location, (OpenTK.Mathematics.Vector2)value);
                break;
            case UniformType.FloatVec3:
                GL.Uniform3(location, (OpenTK.Mathematics.Vector3)value);
                break;
            case UniformType.FloatVec4:
                GL.Uniform4(location, (OpenTK.Mathematics.Vector4)value);
                break;
            case UniformType.IntVec2:
                GL.Uniform2(location, (OpenTK.Mathematics.Vector2i)value);
                break;
            case UniformType.IntVec3:
                GL.Uniform3(location, (OpenTK.Mathematics.Vector3i)value);
                break;
            case UniformType.IntVec4:
                GL.Uniform4(location, (OpenTK.Mathematics.Vector4i)value);
                break;
            case UniformType.FloatMat2:
                {
                    var mat = (OpenTK.Mathematics.Matrix2)value;
                    GL.UniformMatrix2(location, false, ref mat);
                    break;
                }
            case UniformType.FloatMat3:
                {
                    var mat = (OpenTK.Mathematics.Matrix3)value;
                    GL.UniformMatrix3(location, false, ref mat);
                    break;
                }
            case UniformType.FloatMat4:
                {
                    var mat = (OpenTK.Mathematics.Matrix4)value;
                    GL.UniformMatrix4(location, false, ref mat);
                    break;

                }
            case UniformType.Sampler2D:
                {
                    var samplr = (int)value;
                    GL.Uniform1(location, samplr);
                    break;
                }
            default:
                {
                    Logger.LogError($"Unknowh uniform type {type}");
                    return false;
                }
        }
        return true;
    }

    ~ShaderProgramPrototype()
    {
        if (!disposed) Logger.LogError($"Finalizing shader program {Id} without dipsosing!");
    }

    #endregion
}