namespace Simulator.Model.Dtos.Request;

public record EdgeRequestTo(string Id, double Speed, double Distance, string StartPointId, string EndPointId,
    TrafficLightRequestTo? TrafficLight = null)
{ }
