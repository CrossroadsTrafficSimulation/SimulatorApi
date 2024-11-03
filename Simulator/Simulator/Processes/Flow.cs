using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Processes;

public class Flow
{
    public string PointId { get; set; }
    public TimeDensity Density { get; set; }
}

