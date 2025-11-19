using ECS.Events.InputEvents;
using MyOpenTKWindow;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ECS.System.Input;

public class InputSystem : EntitySystem
{
    [SystemDependency] private readonly MyWindow _window = default!;
    private InputState _inputState;

    public override void Update(double deltaT)
    {
        base.Update(deltaT);
        _inputState.keyboardState = _window.KeyboardState;
        _inputState.mouseState = _window.MouseState;

        if (_inputState.keyboardState.IsKeyDown(Keys.D))
        {
            var ev = new RightKeyPressedEvent();
            RaiseEvent(ev);
        }

        if (_inputState.keyboardState.IsKeyDown(Keys.A))
        {
            var ev = new LeftKeyPressedEvent();
            RaiseEvent(ev);
        }

        if (_inputState.keyboardState.IsKeyDown(Keys.W))
        {
            var ev = new UpKeyPressedEvent();
            RaiseEvent(ev);
        }

        if (_inputState.keyboardState.IsKeyDown(Keys.S))
        {
            var ev = new DownKeyPressedEvent();
            RaiseEvent(ev);
        }
    }
}