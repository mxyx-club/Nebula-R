namespace Nebula.Events;

public class LocalEvent
{
    public static List<LocalEvent> Events = new List<LocalEvent>();

    public float duration { get; private set; }
    public bool SpreadOverMeeting { get; protected set; }
    public bool WillStop = true;

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

    public virtual void LocalUpdate()
    {

    }

    protected LocalEvent(float duration)
    {
        this.duration = duration;
    }

    public static void Update()
    {
        foreach (LocalEvent localEvent in Events)
        {
            if (Game.GameData.data.IsTimeStopped && localEvent.WillStop) continue;
            localEvent.LocalUpdate();
            localEvent.duration -= Time.deltaTime;
        }

        Events.RemoveAll(e => e.CheckTerminal());
    }

    public static void Activate(LocalEvent localEvent)
    {
        localEvent.OnActivate();
        Events.Add(localEvent);
    }

    public static void Inactivate(Predicate<LocalEvent> predicate)
    {
        Events.RemoveAll((e) =>
        {
            if (predicate(e))
            {
                e.OnTerminal();
                return true;
            }
            return false;
        });
    }

    public static void Initialize()
    {
        Events.Clear();
    }

    public static void OnMeeting()
    {
        foreach (LocalEvent localEvent in Events)
        {
            if (!localEvent.SpreadOverMeeting)
            {
                localEvent.duration = -1;
            }
        }

        Events.RemoveAll(e => e.CheckTerminal());
    }
}