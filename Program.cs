using System.Diagnostics;
using ECS;
using ECS.Events;
using MegaSuperCoolECS.ECS;
using MyOpenTKWindow;


public sealed class TestSystem() : EntitySystem
{
    [SystemDependency] private HelloWorldSystem _helloWorld = default!;

    private double NextUpdate = 1;
    private double CurrentTime = 0;

    public override void Init()
    {
        base.Init();
        SubscribeEvent<HelloWorldComponent, Event>(OnEvent);

    }
    public override void Update(double deltaT)
    {
        base.Update(deltaT);
        CurrentTime += deltaT;
        if (CurrentTime >= NextUpdate)
        {
            NextUpdate += 0.5;

        }
    }

    public void OnEvent(Component comp, Event ev)
    {
        Console.WriteLine($"This message is printed via Event being raised made at {MathF.Round((float)CurrentTime, 2)}s runtime in {this.GetType()}");
    }
};
public sealed class TimeCounterSystem() : EntitySystem
{
    [SystemDependency] private HelloWorldSystem _helloWorld = default!;

    private double NextUpdate = 1;
    private double CurrentTime = 0;

    public override void Init()
    {
        base.Init();
        SubscribeEvent<HelloWorldComponent, Event>(OnEvent);

    }
    public override void Update(double deltaT)
    {
        base.Update(deltaT);
        CurrentTime += deltaT;
        if (CurrentTime >= NextUpdate)
        {
            NextUpdate += 0.5;

            var ev = new Event();
            RaiseEvent(ev);
        }
    }

    public void OnEvent(Component comp, Event ev)
    {
        Console.WriteLine($"This message is printed via Event being raised made at {MathF.Round((float)CurrentTime, 2)}s runtime in {this.GetType()}");
    }
};

public class HelloWorldComponent : Component
{
    public HelloWorldComponent(int newId) : base(newId)
    {
    }
}

public sealed class HelloWorldSystem() : EntitySystem
{
    public void Print()
    {
        Logger.LogInfo("Hello World!");
    }
}



class Program
{
    public static void Main()
    {
        // Instantiate EntSys
        var sysMan = new EntSysManager();
        sysMan.InitAllSystems(true);

        Logger.LogError("5.1", true, ConsoleColor.DarkRed);

        // Start timer
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastTicks = stopwatch.ElapsedTicks;
        double tickFrequency = Stopwatch.Frequency;

        using (MyWindow window = new(1300, 700, sysMan.UpdateAll))
        {
            window.Run();
        }
    }
}


// public class LicznikEnergii
// {
//     public static float StawkaKW = 100000000;
//     public float Zuzytie = default;

//     public float Koszt
//     {
//         get => Oblicz();
//     }

//     private float Oblicz()
//     {
//         return Zuzytie * StawkaKW;
//     }

//     public static void ZmienStawke(float nowa){
//         StawkaKW = nowa;
//     }
// }

// public class Padadko
// {
//     public static float Stawka = 0.27f;
//     public float CenaNetto = 10;
//     public float CenaBrutto
//     {
//         get => Oblicz();
//     }

//     private float Oblicz()
//     {
//         return CenaNetto * (1 + Stawka);
//     }

//     public static void ZmienStawke(float nowa)
//     {
//         Stawka = nowa;
//     }
// }