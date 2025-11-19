using ECS.Components;
using ECS.Components.Sprite;
using ECS.Components.Transform;
using ECS.Logs;
using ECS.System;
using ECS.System.Sprite;
using ECS.System.Time;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;

namespace MyOpenTKWindow;

[NeedDependencies]
public class MyWindow : GameWindow
{
    [SystemDependency] private readonly EntitySystemManager _sysMan = default!;
    [SystemDependency] private readonly ComponentManager _compMan = default!;
    [SystemDependency] private readonly TimeSystem _time = default!;
    [SystemDependency] private readonly SpriteSystem _sprite = default!;

    public Vector2i ScreenSize
    {
        get => _screenSize;
        set
        {
            _screenSize = value;
            _aspect = (float)value.X / (float)value.Y;
            Shader?.SetAspectUniform(_aspect);
            GL.Viewport(0, 0, value.X, value.Y);
        }
    }
    private Vector2i _screenSize;

    public float Apsect { get => _aspect; }
    private float _aspect;

    private float t = 0;

    private Shader? Shader { get; set; }

    private int[] _indecies = {
            0,1,3,
            1,2,3};
    private float[] vertices = {
            0.5f,  0.5f, 0.0f, 1f, 1f,      // top right
            0.5f, -0.5f, 0.0f, 1f, 0f,       // bottom right
           -0.5f, -0.5f, 0.0f, 0f, 0f,       // bottom left
           -0.5f,  0.5f, 0.0f, 0f, 1f,      // top left
                            };

    private int _vbo;
    private int _vao;
    private int _ebo;

    // Parametrless constructor for Activator.CreateInstance(Type);
    public MyWindow() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
    }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="w">Width of the screen</param>
    /// <param name="h">Height of the screen</param>
    public MyWindow(int w, int h) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        Init(w, h);
    }
    public void Init(int w, int h)
    {
        try
        {
            Shader = new Shader("Shaders/basic.vert", "Shaders/basic.frag");
        }
        catch (Exception ex)
        {
            Logger.LogFatal($"Failed to load shaders: {ex.Message} {ex.InnerException?.Message}");
            this.Close();
        }
        Shader!.Use();
        this._screenSize = new(w, h);
        this.CenterWindow(ScreenSize);
        GL.Viewport(this.Location.X, this.Location.Y, w, h);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.2f, 0.2f, 0);


        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indecies.Length * sizeof(uint), _indecies, BufferUsageHint.StaticDraw);

        // int posAttribLocation = GL.GetAttribLocation(Shader!.Handle, "aPosition");
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // int texCoordLocation = GL.GetAttribLocation(Shader!.Handle, "aTexCoord");
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

    }
    protected override void OnUnload()
    {
        base.OnUnload();
        Shader!.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        _sysMan.UpdateAll(args.Time);
        Logger.PrintQueue();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        t = (float)_time.Time.TotalSeconds;
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ClearColor(0.2f, 0.2f, 0.2f, 0);

        foreach (var transform in _compMan.Components.OfType<TransformComponent>())
        {
            if (_compMan.TryGetComp<SpriteComponent>(transform.OwnerID, out var sprite))
            {
                if (sprite is null || sprite.Image is null) continue;
                // Logger.LogInfo($"Proccessing texture with id: {sprite.TextureID} and data size: {sprite.image.Data.Count()}bytes");
                var texUniform = GL.GetUniformLocation(Shader!.Handle, "texture0");

                _sprite.UpdateSprite(sprite);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, sprite.TextureID);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.Uniform1(texUniform, 0);
            }

            // adjust pivot to center


            var halfW = transform.Size.X / 2;
            var halfH = transform.Size.Y / 2;

            var x1 = transform.Position.X - halfW;
            var x2 = transform.Position.X + halfW;
            var y1 = transform.Position.Y - halfH;
            var y2 = transform.Position.Y + halfH;


            vertices = [
                x2, y2, 0, 1f, 1f, // top right
                x2, y1, 0, 1f, 0f, // bottom right
                x1, y1, 0, 0f, 0f, // bottom left
                x1, y2, 0, 0f, 1f, // top left
            ];

            // Ensure binding
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.DrawElements(PrimitiveType.Triangles, _indecies.Length, DrawElementsType.UnsignedInt, 0);
        }


        // 3. now draw the object
        // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        this.SwapBuffers();
    }
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        ScreenSize = e.Size;
        GL.Viewport(this.Location.X, this.Location.Y, e.Width, e.Height);
    }

    public int RequestTexture()
    {
        return GL.GenTexture();
    }
}

public class Shader : IDisposable
{
    public int Handle;

    public int VertexShader;
    public int FragmentShader;

    private int AspectUniformLocation;

    private bool disposedValue = false;

    ~Shader()
    {
        if (!disposedValue)
        {
            Logger.LogError("Shader is not disposed after finalization!");
        }
    }

    public Shader(string vertPath, string fragPath)
    {
        // Get source code
        string vertSource = File.ReadAllText(vertPath);
        string fragSource = File.ReadAllText(fragPath);

        // Generate shaders
        VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, vertSource);

        FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, fragSource);

        // Compile shaders
        GL.CompileShader(VertexShader);

        GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(VertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(FragmentShader);

        GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(FragmentShader);
            Console.WriteLine(infoLog);
        }

        // Create GPU program and attach shaders
        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        // Detach and delete shaders once program is linked
        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);

        // Get the locations of uniforms.
        AspectUniformLocation = GL.GetUniformLocation(this.Handle, "aspect");
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void SetAspectUniform(float aspect)
    {
        GL.Uniform1(this.AspectUniformLocation, aspect);
    }
}
