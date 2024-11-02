using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.RequestDto;

public record Flow(string PointId, TimeDensity Density) { }
