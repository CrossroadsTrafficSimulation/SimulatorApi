using Simulator.Model;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;

namespace Simulator.Processes;

public class Simulation
{
    public List<Flow> Flows { get; set; } = null!;
    public List<Edge> Edges { get; set; } = null!;
    public List<Point> Points { get; set; } = null!;
    public List<TrafficLight> TrafficLights { get; set; } = null!;

    private readonly List<Event> _events = [];

    public void ProcessVehicles(int currentTime)
    {
        var vehicleEvents = _events.Where(e => e.Type.Equals("Vehicle", StringComparison.OrdinalIgnoreCase));
        throw new NotImplementedException();
    }
    public void ProcessTrafficLights(int currentTime)
    {
        var trafficLightsEvents = _events.Where(e => e.Type.Equals("TrafficLight", StringComparison.OrdinalIgnoreCase) && e.EndTime == currentTime).ToList();
        foreach (var e in trafficLightsEvents)
        {
            e.Action();
        }

    }

    public void PrecessPedestrians(int currentTime)
    {
        throw new NotImplementedException();
    }

    public SimulationResponseTo GetResult()
    {
        throw new NotImplementedException();
    }

    private void ChangeTrafficLightState(TrafficLight? trafficLight, int NextActionTime)
    {
        /*            if (trafficLight == null)
                        throw new ArgumentNullException($"Traffic light is null");
                    trafficLight.CurrentState++;
                    trafficLight.CurrentState %= trafficLight.States.Count;
                    _events.Add(new Event() 
                    {
                        Action = () => ChangeTrafficLightState(trafficLight, NextActionTime + trafficLight.States[trafficLight.CurrentState]),
                        Duration = trafficLight.CurrentState,
                        EndTime = NextActionTime + trafficLight.States[trafficLight.CurrentState],
                        StartTime = NextActionTime,
                        Type = "TrafficLight"
                    });*/
    }
}
