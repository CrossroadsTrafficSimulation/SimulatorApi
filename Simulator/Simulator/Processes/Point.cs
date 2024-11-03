using Simulator.Model;
using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Processes;

public class Point
{
    public string Id { get; set; }
    public TimeDensity? PedestriansFlow { get; set; }

    private Queue<Vehicle> _vehicles = new Queue<Vehicle>();
    

}

