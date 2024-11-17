using Simulator.Model.Enums;

namespace Simulator.Model;

internal class Event
{
    public EventType Type { get; set; }
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public int Duration { get; set; }
    public Action Action { get; set; } = null!;
}

