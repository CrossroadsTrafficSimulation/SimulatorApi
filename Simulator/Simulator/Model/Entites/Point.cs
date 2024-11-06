using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Entites;

public class Point()
{
    public string Id { get; set; } = null!;
    public List<Edge> Edges { get; set; } = [];
    public Flow Flow { get; set; } = null!;
    public TimeDensity? PedestriansFlow { get; set; } = null;

    public Point(string id, List<Edge> edges, Flow flow, TimeDensity? pedestriansFlow) : this()
    {
        Id = id;
        Edges = edges;
        Flow = flow;
        PedestriansFlow = pedestriansFlow;
    }
}
