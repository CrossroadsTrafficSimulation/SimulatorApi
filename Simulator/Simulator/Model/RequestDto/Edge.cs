namespace Simulator.Model.RequestDto;

public record Edge(double Speed, double Distance, string StartPointId, string EndPointId, TrafficLight? TrafficLight) { }
