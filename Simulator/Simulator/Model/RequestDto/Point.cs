using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.RequestDto;

public record Point(string Id, TimeDensity? PedestriansFlow)
{
    //public Queue<Vehicle> VehicleQueue { get; set; } = new Queue<Vehicle>();
}
