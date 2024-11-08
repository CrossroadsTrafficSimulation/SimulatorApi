using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;

namespace Simulator.Processes;

public class Simulation
{
    public SimulationModel SimulationModel { get; set; } = null!;

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
            _events.Remove(e);
        }

    }

    public void ProcessPedestrians(int currentTime)
    {
        throw new NotImplementedException();
    }

    public SimulationResponseTo GetResult()
    {
        throw new NotImplementedException();
    }

    private void ChangeTrafficLightState(TrafficLight? trafficLight, int EndOfAction)
    {
        ArgumentNullException.ThrowIfNull(trafficLight);

        if (trafficLight.CurrentState == TrafficLightState.Green)
        {
            trafficLight.PreviousState = TrafficLightState.Green;
            trafficLight.CurrentState = TrafficLightState.Yellow;
        }
        else if (trafficLight.CurrentState == TrafficLightState.Red)
        {
            trafficLight.PreviousState = TrafficLightState.Red;
            trafficLight.CurrentState = TrafficLightState.Yellow;
        }
        else
        {
            trafficLight.CurrentState = trafficLight.PreviousState == TrafficLightState.Green ? TrafficLightState.Red : TrafficLightState.Green;
            trafficLight.PreviousState = TrafficLightState.Yellow;

        }

        //Console.WriteLine($"Traffic light changed Color from {trafficLight.PreviousState} to {trafficLight.CurrentState}, Event was created at {EndOfAction}");

        _events.Add(new Event()
        {
            Action = () => ChangeTrafficLightState(trafficLight, EndOfAction + trafficLight.States[trafficLight.CurrentState]),
            Duration = trafficLight.States[trafficLight.CurrentState],
            EndTime = EndOfAction + trafficLight.States[trafficLight.CurrentState],
            StartTime = EndOfAction,
            Type = "TrafficLight"
        });
    }

    public void SetUpFirstEvents()
    {
        foreach (var trafficLight in SimulationModel.TrafficLights)
        {
            _events.Add(new Event()
            {
                Action = () => ChangeTrafficLightState(trafficLight, trafficLight.States[trafficLight.CurrentState]),
                Duration = trafficLight.States[trafficLight.CurrentState],
                EndTime = trafficLight.States[trafficLight.CurrentState],
                StartTime = 0,
                Type = "TrafficLight"
            });
        }
    }

}
