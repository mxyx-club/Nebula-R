namespace Nebula.Events;

internal class Schedule
{
    private static List<Tuple<int, Action>> PreMeetingActions = new List<Tuple<int, Action>>();
    private static List<Tuple<int, Action>> PostMeetingActions = new List<Tuple<int, Action>>();

    public static void Initialize()
    {
        PreMeetingActions.Clear();
        PostMeetingActions.Clear();
    }

    public static void RegisterPreMeetingAction(Action action, int priority)
    {
        PreMeetingActions.Add(new Tuple<int, Action>(priority, action));
    }

    public static void RegisterPostMeetingAction(Action action, int priority)
    {
        PostMeetingActions.Add(new Tuple<int, Action>(priority, action));
    }

    public static void OnPreMeeting()
    {
        PreMeetingActions.Sort((a, b) => (a.Item1 - b.Item1));
        foreach (var tuple in PreMeetingActions)
        {
            tuple.Item2.Invoke();
        }
        PreMeetingActions.Clear();
    }

    public static void OnPostMeeting()
    {
        PostMeetingActions.Sort((a, b) => (a.Item1 - b.Item1));
        foreach (var tuple in PostMeetingActions)
        {
            tuple.Item2.Invoke();
        }
        PostMeetingActions.Clear();
    }
}