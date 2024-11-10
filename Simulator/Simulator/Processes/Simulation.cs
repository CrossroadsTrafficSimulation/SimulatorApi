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
        Console.WriteLine();
        Console.WriteLine($"Current time: {currentTime}");
        foreach (var p in SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null))
        {
            Console.WriteLine($"Pedestrians: {p.Key} - {p.Value.PedestriansOnTheRoad} - {p.Value.NumberOfPedestrians}");
        }
        //foreach (var )
    }

    public SimulationResponseTo GetResult()
    {
        throw new NotImplementedException();
    }

    private void ChangeTrafficLightState(TrafficLight? trafficLight, int endOfAction)
    {
        ArgumentNullException.ThrowIfNull(trafficLight);

        // !!! MOVE TO TrafficLight CLASS METHOD !!!
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

        //Console.WriteLine($"Traffic light changed Color from {trafficLight.PreviousState} to {trafficLight.CurrentState}, Event was created at {EndOfAction} at edge {trafficLight.EdgeId}");
        //Console.WriteLine();
        //foreach (var tr in SimulationModel.TrafficLights)
        //{
        //    Console.WriteLine($"Traffic light: {tr.EdgeId,-30}, Color: {tr.CurrentState} at {EndOfAction}");
        //}

        _events.Add(new Event()
        {
            Action = () => ChangeTrafficLightState(trafficLight, endOfAction + trafficLight.States[trafficLight.CurrentState]),
            Duration = trafficLight.States[trafficLight.CurrentState],
            EndTime = endOfAction + trafficLight.States[trafficLight.CurrentState],
            StartTime = endOfAction,
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

        // !!! REPLACED !!!
        /*        var pedestrianFlows = SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null).ToList();
                var pedestrianFlowsList = new List<List<Point>>();

                while (pedestrianFlows.Count > 0)
                {
                    var point = pedestrianFlows.First().Value;
                    var points = pedestrianFlows
                        .Where(p => p.Value.PedestriansFlow!.Equals(point.PedestriansFlow))
                        .ToList();
                    pedestrianFlowsList.Add(points.Select(p => p.Value).ToList());
                    foreach (var p in points)
                    {
                        pedestrianFlows.Remove(p);
                    }
                }*/
        // !!! REPLACED !!!
        var pedestrianFlowsList = SimulationModel.Points
            .Where(p => p.Value.PedestriansFlow is not null)
            .GroupBy(p => p.Value.PedestriansFlow)
            .Select(group => group.Select(p => p.Value).ToList())
            .ToList();
        // !!! REPLACED !!!

        foreach (var points in pedestrianFlowsList)
        {

            _events.Add(new Event()
            {
                Action = () => GeneratePedestrian(points, endTime: 60),
                Duration = 60,
                StartTime = 0,
                EndTime = 60,
                Type = "Pedestrian"
            });
        }
    }

    public void GeneratePedestrian(List<Point> crosswalk, int endTime)
    {
        _events.Add(new Event()
        {
            Action = () => GeneratePedestrian(crosswalk, endTime + 60),
            Duration = 60,
            StartTime = endTime,
            EndTime = endTime + 60,
            Type = "Pedestrian"
        });

        var flow = crosswalk
            .FirstOrDefault()?.PedestriansFlow?
            .Where(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second <= endTime)
            .Select(p => p.Value)
            .LastOrDefault();
        //Console.WriteLine($"Current flow generated: {flow.Value} for point: {points.Id} at {EndTime}");

        if (flow is null || flow.Value == 0)
        {
            return;
        }

        var trafficLights = crosswalk.SelectMany(p => p.TrafficLights).ToList();

        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            var rand = new Random();
            foreach (var point in crosswalk)
            {
                point.PedestriansOnTheRoad += flow.Value;
            }
            
            for (int i = 0; i < flow.Value; i++)
            {
                var crossingTime = rand.Next(5, 10) * crosswalk.Count;
                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = endTime + crossingTime,
                    StartTime = endTime,
                    Type = "Pedestrian"
                });
            }
        }
        else
        {
            foreach (var point in crosswalk)
            {
                point.NumberOfPedestrians += flow.Value;
            }

            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, endTime + 1),
                Duration = 1,
                EndTime = endTime + 1,
                StartTime = endTime,
                Type = "Pedestrian"
            });
        }
        Console.WriteLine();

       
        //Console.WriteLine("")
    }

    public void StartCrossTheRoad(List<Point> crosswalk, List<TrafficLight> trafficLights, int EndTime)
    {
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            foreach (var point in crosswalk)
            {
                point.PedestriansOnTheRoad += point.NumberOfPedestrians;
            }

            var rand = new Random();

            for (int i = 0; i < crosswalk.FirstOrDefault()?.NumberOfPedestrians; i++)
            {
                var crossingTime = rand.Next(5, 10) * crosswalk.Count;

                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = EndTime + crossingTime,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
            foreach (var point in crosswalk)
            {
                point.NumberOfPedestrians = 0;
            }

        }
        else
        {
            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, EndTime + 1),
                Duration = 1,
                EndTime = EndTime + 1,
                StartTime = EndTime,
                Type = "Pedestrian"
            });
        }
    }

    public void CrossTheRoad(List<Point> crosswalk)
    {
        foreach (var point in crosswalk)
        {
            if (point.PedestriansOnTheRoad < 1.0d)
            {
                return;
            }
            point.PedestriansOnTheRoad--;
        }
       
    }

}
