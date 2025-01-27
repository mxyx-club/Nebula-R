namespace Nebula.Events;

public delegate GlobalEvent GlobalEventGenerator(float duration, ulong option);
public class GlobalEvent
{
    public static List<GlobalEvent> Events = new List<GlobalEvent>();
    private static Dictionary<Type, GlobalEventGenerator> Generators = new Dictionary<Type, GlobalEventGenerator>();

    public class Type
    {
        private static byte availableId = 0;
        public static Type Camouflage = new Type();
        public static Type EMI = new Type();
        public static Type BlackOut = new Type();

        public byte Id { get; }

        private Type()
        {
            Id = availableId;
            availableId++;
        }

        public static HashSet<Type> AllTypes = new HashSet<Type>()
            {
                Camouflage,
                EMI,
                BlackOut
            };

        public static Type GetType(byte id)
        {
            foreach (Type type in AllTypes)
            {
                if (type.Id == id)
                {
                    return type;
                }
            }
            return null;
        }
    }

    public Type type { get; }
    public float duration { get; private set; }
    public bool SpreadOverMeeting { get; protected set; }

    public bool CheckTerminal()
    {
        if (duration < 0)
        {
            OnTerminal();
            return true;
        }
        return false;
    }

    public virtual void OnTerminal()
    {

    }

    public virtual void OnActivate()
    {

    }

    public virtual void Update(float left)
    {

    }

    protected GlobalEvent(Type type, float duration, ulong option)
    {
        this.type = type;
        this.duration = duration;
    }

    public static bool IsActive(Type type)
    {
        foreach (GlobalEvent globalEvent in Events)
        {
            if (globalEvent.type != type) continue;
            if (globalEvent.duration > 0) return true;
        }
        return false;
    }

    public static void Update()
    {
        if (Game.GameData.data.IsTimeStopped) return;
        foreach (GlobalEvent globalEvent in Events)
        {
            globalEvent.duration -= Time.deltaTime;
            globalEvent.Update(globalEvent.duration);
        }

        Events.RemoveAll(e => e.CheckTerminal());
    }

    public static bool Activate(Type type, float duration, ulong option)
    {
        if (Generators.ContainsKey(type))
        {
            GlobalEvent e = Generators[type](duration, option);
            e.OnActivate();
            Events.Add(e);
            return true;
        }
        return false;
    }

    public static void Register(Type type, GlobalEventGenerator generator)
    {
        Generators.Add(type, generator);
    }

    public static void Initialize()
    {
        Events.Clear();
    }

    public static void OnMeeting()
    {
        foreach (GlobalEvent globalEvent in Events)
        {
            if (!globalEvent.SpreadOverMeeting)
            {
                globalEvent.duration = -1;
            }
        }

        Events.RemoveAll(e => e.CheckTerminal());
    }
}
