using Simulator.Model.Enums;

namespace Simulator.Model.Dtos.Request;

public record TrafficLightRequestTo(int RedSeconds, int YellowSeconds, int GreenSeconds, TrafficLightState InitialState = TrafficLightState.Red) { }
