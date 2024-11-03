namespace Simulator.Model;

public class Event
{
    public string Type { get; set; }
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public int Duration { get; set; }
    public Action Action { get; set; }
}

