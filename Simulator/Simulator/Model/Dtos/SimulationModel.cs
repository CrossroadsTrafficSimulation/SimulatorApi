using Simulator.Model.Entites;

using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;

namespace Simulator.Model.Dtos;

public record SimulationModel(Dictionary<string, Point> Points, List<Edge> Edges, List<Flow> Flows, List<TrafficLight> TrafficLights,
    List<Route> Routes)
{ }
