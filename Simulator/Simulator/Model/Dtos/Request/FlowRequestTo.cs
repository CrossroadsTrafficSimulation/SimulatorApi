using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Dtos.Request;

public record FlowRequestTo(string PointId, TimeDensity Density) { }
