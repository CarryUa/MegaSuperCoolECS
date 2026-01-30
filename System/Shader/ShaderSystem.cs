using ECS.Logs;
using ECS.Prototypes;
using ECS.Prototypes.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace ECS.System.Shader;

public class ShaderSystem : EntitySystem
{
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;

    public void CompileAllShaders()
    {
        static void TryCompileShader(ShaderPrototype shader)
        {
            try
            {
                string source = File.ReadAllText(shader.ShaderPath);
                shader.ShaderID = GL.CreateShader(shader.ShaderType);
                GL.ShaderSource(shader.ShaderID, source);

                GL.CompileShader(shader.ShaderID);

                GL.GetShader(shader.ShaderID, ShaderParameter.CompileStatus, out int success);
                if (success == 0) throw new Exception($"\n{GL.GetShaderInfoLog(shader.ShaderID)}");

            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't compile shader ('{shader.ShaderPath}'): {e.Message}");
            }
        }

        var targets = _protoMan.GetPrototypes<ShaderPrototype>();

        foreach (var target in targets)
        {
            TryCompileShader(target);
        }
    }

    public void CreateAllPrograms()
    {
        Dictionary<string, (AttributeType, int)> GetAttributeLocations(ShaderProgramPrototype program, string[] shaderProtoIDs)
        {
            var result = new Dictionary<string, (AttributeType, int)>();


            foreach (var shaderProtoID in shaderProtoIDs)
            {
                var shader = _protoMan.GetPrototype<ShaderPrototype>(shaderProtoID);
                foreach (var (attribute, type) in shader.Attributes)
                {
                    result.TryAdd(attribute, (type, GL.GetAttribLocation(program.Handle, attribute)));
                }

            }

            return result;
        }
        Dictionary<string, (UniformType, int)> GetUniformsLocations(ShaderProgramPrototype program, string[] shaderProtoIDs)
        {
            var result = new Dictionary<string, (UniformType, int)>();


            foreach (var shaderProtoID in shaderProtoIDs)
            {
                var shader = _protoMan.GetPrototype<ShaderPrototype>(shaderProtoID);
                foreach (var (uniform, type) in shader.Uniforms)
                {
                    result.TryAdd(uniform, (type, GL.GetUniformLocation(program.Handle, uniform)));
                }

            }

            return result;
        }
        void LinkProgram(ShaderProgramPrototype program, string[] shaderProtoIDs)
        {
            var toDetach = new List<int>();
            foreach (var shaderProtoID in shaderProtoIDs)
            {
                try
                {
                    var shader = _protoMan.GetPrototype<ShaderPrototype>(shaderProtoID);

                    Logger.LogDebug($"Attaching shader {shaderProtoID} to program {program.Id}");
                    GL.AttachShader(program.Handle, shader.ShaderID);

                    toDetach.Add(shader.ShaderID);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Could't link shader('{shaderProtoID}') to program('{program.Id}'): {e.Message}");
                }
            }
            GL.LinkProgram(program.Handle);
            GL.GetProgram(program.Handle, GetProgramParameterName.LinkStatus, out int success);

            if (success == 0) Logger.LogError($"Could't link some or all shaders to program('{program.Id}'): {GL.GetProgramInfoLog(program.Handle)} (code {success})", false);
            else Logger.LogInfo($"Program linking status: {success}; Log: {GL.GetProgramInfoLog(program.Handle)}; Linked shaders count: {toDetach.Count}");

            foreach (var shader in toDetach)
            {
                GL.DetachShader(program.Handle, shader);
            }
        }

        var targets = _protoMan.GetPrototypes<ShaderProgramPrototype>();

        foreach (var target in targets)
        {
            target.Handle = GL.CreateProgram();
            LinkProgram(target, [target.VertexShader, target.FragmentShader]);
            target.UniformLocations = GetUniformsLocations(target, [target.VertexShader, target.FragmentShader]);
            target.AttributeLocations = GetAttributeLocations(target, [target.VertexShader, target.FragmentShader]);
        }
    }

    public void DeleteAllShaders()
    {
        var targets = _protoMan.GetPrototypes<ShaderPrototype>();

        foreach (var target in targets)
        {
            GL.DeleteShader(target.ShaderID);
        }
    }

    public void DisposeAllPrograms()
    {
        var targets = _protoMan.GetPrototypes<ShaderProgramPrototype>();
        foreach (var target in targets)
        {
            target.Dispose();
        }
    }
}