using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;
using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

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
        var pedestriansEvents = _events.Where(e => e.Type.Equals("Pedestrian", StringComparison.OrdinalIgnoreCase) && e.EndTime == currentTime).ToList();
        foreach (var e in pedestriansEvents)
        {
            e.Action();
            _events.Remove(e);
        }
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
        foreach (var point in SimulationModel.Points)
        {
            if (point.Value.PedestriansFlow?.Count != 0)
            {
                _events.Add(new Event()
                {
                    Action = () => GeneratePedestrian(point.Value, 60),
                    Duration = 60,
                    StartTime = 0,
                    EndTime = 60,
                    Type = "Pedestrian"
                });
            }

        }
    }

    public void GeneratePedestrian(Point point, int EndTime)
    {
        var flow = point.PedestriansFlow?
            .Where(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second <= EndTime)
            .OrderBy(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second)
            .Select(p => p.Value)
            .LastOrDefault();
        //Console.WriteLine($"Current flow generated: {flow.Value} for point: {point.Id} at {EndTime}");

        _events.Add(new Event()
        {
            Action = () => GeneratePedestrian(point, EndTime + 60),
            Duration = 60,
            StartTime = EndTime,
            EndTime = EndTime + 60,
            Type = "Pedestrian"
        });
        if (flow.Value == 0)
        {
            return;
        }

        var trafficLight = point.Edges
            .FirstOrDefault(e => e.EndPoint.Id == point.Id)?
            .TrafficLight;
        if (trafficLight?.CurrentState == TrafficLightState.Green)
        {
            var rand = new Random();
            point.PedestriansOnTheRoad += flow.Value;
            for (int i = 0; i < flow.Value; i++)
            {
                var crossingTime = rand.Next(5, 15);
                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(point),
                    Duration = crossingTime,
                    EndTime = EndTime + crossingTime,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
        }
        else
        {
            point.NumberOfPedestrians += flow.Value;
            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(point,trafficLight!,EndTime),
                Duration = 1,
                EndTime = EndTime + 1,
                StartTime = EndTime,
                Type = "Pedestrian"
            });
        }

    }

    public void StartCrossTheRoad(Point point,TrafficLight trafficLight, int EndTime)
    {
        if (trafficLight.CurrentState == TrafficLightState.Green)
        {
            point.PedestriansOnTheRoad += point.NumberOfPedestrians;
            var rand = new Random();
            for (int i = 0; i < point.NumberOfPedestrians; i++)
            {
                var crossingTime = rand.Next(5, 15);
                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(point),
                    Duration = crossingTime,
                    EndTime = EndTime + crossingTime,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
            point.NumberOfPedestrians = 0;

        }
        else
        {
            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(point,trafficLight ,EndTime),
                Duration = 1,
                EndTime = EndTime + 1,
                StartTime = EndTime,
                Type = "Pedestrian"
            });
        }
    }

    public void CrossTheRoad(Point point)
    {
        if (point.PedestriansOnTheRoad < 1.0d)
        {
            return;
        }
        point.PedestriansOnTheRoad--;
    }

}
