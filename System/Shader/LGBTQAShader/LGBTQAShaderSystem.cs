using ECS.Events.ShaderEvents;
using ECS.Logs;
using ECS.System.Time;
using MyOpenTKWindow;

namespace ECS.System.Shader.LGBTQAShader;

public class LGBTQAShaderSystem : EntitySystem
{
    [SystemDependency] private readonly TimeSystem _time = default!;
    [SystemDependency] private readonly MyWindow _window = default!;

    public override void PreInit()
    {
        base.PreInit();
        SubscribeEvent<ShaderProgramPreRenderEvent>(OnPreRender);
    }

    public void OnPreRender(ShaderProgramPreRenderEvent ev)
    {
        if (ev.ShaderProgramPrototype.FragmentShader != "LGBTQAFragmentShader") return;
        try
        {
            // Logger.LogDebug($"Setting aspect to {_window.Apsect}");
            ev.ShaderProgramPrototype.TrySetUniform("aspect", OpenTK.Graphics.OpenGL4.UniformType.Float, _window.Apsect);
            // Logger.LogDebug($"Setting sampler to {0}");
            ev.ShaderProgramPrototype.TrySetUniform("texture0", OpenTK.Graphics.OpenGL4.UniformType.Sampler2D, 0);
            // Logger.LogDebug($"Setting time to {(float)_time.Time.TotalSeconds}");
            ev.ShaderProgramPrototype.TrySetUniform("time", OpenTK.Graphics.OpenGL4.UniformType.Float, (float)_time.Time.TotalSeconds);
            ev.ShaderProgramPrototype.TrySetUniform("lgbtqa_speed", OpenTK.Graphics.OpenGL4.UniformType.Float, 10f);

        }
        catch (Exception ex)
        {
            LogError(ex);
        }


    }
}