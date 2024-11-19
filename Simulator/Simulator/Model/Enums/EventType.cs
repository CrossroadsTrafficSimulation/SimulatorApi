namespace Simulator.Model.Enums;

public enum EventType
{
    TrafficLightSwitch,

    PedestrianGenerate,
    PedestrianCross,
    PedestrianStartCross,

    VehicleGenerate,
    VehicleMove,
    VehicleMoveToNextEdge,
    VehicleAddGenerated,

    UpdateStatistics,
    CollectStatistics
}
