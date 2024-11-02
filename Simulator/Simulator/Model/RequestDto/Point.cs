using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.RequestDto;

public record Point(string Id, TrafficLight? TrafficLight, TimeDensity? PedestriansFlow) { }
