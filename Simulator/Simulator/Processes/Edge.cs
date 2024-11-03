using Simulator.Model.RequestDto;

namespace Simulator.Processes;

public class Edge
{
    public double Speed { get; set; }
    public double Distance { get; set; }
    public string StartPointId { get; set; }
    public string EndPointId { get; set; }
    public TrafficLight? TrafficLight { get; set; }
}

