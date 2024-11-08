using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Dtos.Request;

public record PedestrianFlowRequestTo(List<string> PointIds, TimeDensity Density) { }
