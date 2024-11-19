using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Entites;

public class Point()
{
    public string Id { get; set; } = null!;
    public List<Edge> Edges { get; set; } = [];
    public List<TrafficLight> TrafficLights { get; set; } = [];
    public Flow Flow { get; set; } = null!;
    public TimeDensity? PedestriansFlow { get; set; } = null;
    /// <summary>
    /// NumberOfPedestrians
    /// </summary>
    public double PedestriansQueueCount { get; set; } = 0.0;
    public double PedestriansOnTheRoadCount { get; set; } = 0.0;

    public Point(string id, List<Edge> edges, Flow flow, TimeDensity? pedestriansFlow) : this()
    {
        Id = id;
        Edges = edges;
        Flow = flow;
        PedestriansFlow = pedestriansFlow;
    }

    public bool IsPossibleToDriveThrough()
    {
        return PedestriansOnTheRoadCount < 1.0;
    }
}
