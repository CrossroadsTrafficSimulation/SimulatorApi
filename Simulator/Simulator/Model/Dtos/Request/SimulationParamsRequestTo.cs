using RouteRequestTo = System.Collections.Generic.List<string>;

namespace Simulator.Model.Dtos.Request;

public record SimulationParamsRequestTo(List<PointRequestTo> Points, List<EdgeRequestTo> Edges, List<FlowRequestTo> Flows,
    List<RouteRequestTo> Routes)
{ }
