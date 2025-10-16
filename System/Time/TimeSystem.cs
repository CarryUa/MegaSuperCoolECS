namespace ECS.System.Time;


[InitializationPriority(InitPriority.High)]
class TimeSystem : EntitySystem
{
    /// <summary>
    /// The time elapsed since this system was started.
    /// </summary>
    public TimeSpan Time { get => _time; }

    /// <summary>
    /// The time elapsed since the last update.
    /// </summary>
    public TimeSpan DeltaTime { get => _deltaTime; }

    /// <summary>
    /// The average delay time between updates.
    /// </summary>
    public TimeSpan FrameRate { get => _frameRate; }

    /// <summary>
    /// The total number of updates since this system was started.
    /// </summary>
    public ulong FrameCount { get => _frameCount; }

    private TimeSpan _time = TimeSpan.Zero;
    private TimeSpan _deltaTime = TimeSpan.Zero;
    private TimeSpan _frameRate = TimeSpan.Zero;
    private ulong _frameCount = 0;

    public override void Update(double deltaT)
    {
        base.Update(deltaT);
        _frameCount++;
        _time += TimeSpan.FromSeconds(deltaT);
        _deltaTime = TimeSpan.FromSeconds(deltaT);
        _frameRate = TimeSpan.FromSeconds(_time.TotalSeconds / _frameCount);
    }
}