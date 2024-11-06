using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Dtos.Request;

public record PointRequestTo(string Id, TimeDensity? PedestriansFlow) { }
