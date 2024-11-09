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

        //Console.WriteLine($"Traffic light changed Color from {trafficLight.PreviousState} to {trafficLight.CurrentState}, Event was created at {EndOfAction} at edge {trafficLight.EdgeId}");
        //Console.WriteLine();
        //foreach (var tr in SimulationModel.TrafficLights)
        //{
        //    Console.WriteLine($"Traffic light: {tr.EdgeId,-30}, Color: {tr.CurrentState} at {EndOfAction}");
        //}

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
        var tempFlows = SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null && p.Value.PedestriansFlow?.Count != 0).ToList();
        var listOfFlows = new List<List<Point>>();
        while (tempFlows.Count > 0)
        {
            var point = tempFlows
                .FirstOrDefault();
            var points = tempFlows
                .Where(p => p.Value.PedestriansFlow!.Equals(point.Value.PedestriansFlow))
                .ToList();
            listOfFlows.Add(points.Select(p => p.Value).ToList());
            foreach (var p in points)
            {
                tempFlows.Remove(p);
            }
        }

        foreach (var points in listOfFlows)
        {

            _events.Add(new Event()
            {
                Action = () => GeneratePedestrian(points, 60),
                Duration = 60,
                StartTime = 0,
                EndTime = 60,
                Type = "Pedestrian"
            });


        }
    }

    public void GeneratePedestrian(List<Point> points, int EndTime)
    {
        var flow = points
            .FirstOrDefault()?
            .PedestriansFlow?
            .Where(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second <= EndTime)
            .OrderBy(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second)
            .Select(p => p.Value)
            .LastOrDefault();
        //Console.WriteLine($"Current flow generated: {flow.Value} for point: {points.Id} at {EndTime}");

        _events.Add(new Event()
        {
            Action = () => GeneratePedestrian(points, EndTime + 60),
            Duration = 60,
            StartTime = EndTime,
            EndTime = EndTime + 60,
            Type = "Pedestrian"
        });
        if (flow.Value == 0)
        {
            return;
        }
        var edges = points
            .Join(SimulationModel.Edges, p => p.Id, e => e.StartPointId, (p,e) =>new {Edge = e})
            .Where(e => e.Edge.TrafficLight is not null)
            .Select(e => e.Edge.Id)
            .ToList();
        var trafficLights = SimulationModel
            .TrafficLights
            .Where(t => edges.Contains(t.EdgeId))
            .ToList();
        var isPossibleToCross = true;
        foreach (var tr in trafficLights)
        {
            if (tr.CurrentState != TrafficLightState.Red)
            {
                isPossibleToCross = false;
                break;
            }
        }
        if (isPossibleToCross)
        {
            var rand = new Random();
            foreach (var point in points)
            {
                point.PedestriansOnTheRoad += flow.Value;
            }
            
            for (int i = 0; i < flow.Value; i++)
            {
                var crossingTime = rand.Next(5, 10) * points.Count;
                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(points),
                    Duration = crossingTime,
                    EndTime = EndTime + crossingTime,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
        }
        else
        {
            foreach (var point in points)
            {
                point.NumberOfPedestrians += flow.Value;
            }
            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(points, trafficLights!, EndTime + 1),
                Duration = 1,
                EndTime = EndTime + 1,
                StartTime = EndTime,
                Type = "Pedestrian"
            });
        }
        Console.WriteLine();

       
        //Console.WriteLine("")



    }

    public void StartCrossTheRoad(List<Point> points, List<TrafficLight> trafficLights, int EndTime)
    {
        var isPossibleToCross = true;
        foreach (var tr in trafficLights)
        {
            if (tr.CurrentState != TrafficLightState.Red)
            {
                isPossibleToCross = false;
                break;
            }
        }
        if (isPossibleToCross)
        {
            foreach (var point in points)
            {
                point.PedestriansOnTheRoad += point.NumberOfPedestrians;
            }
            var rand = new Random();
            for (int i = 0; i < points.FirstOrDefault()?.NumberOfPedestrians; i++)
            {
                var crossingTime = rand.Next(5, 10)*points.Count;
                _events.Add(new Event()
                {
                    Action = () => CrossTheRoad(points),
                    Duration = crossingTime,
                    EndTime = EndTime + crossingTime,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
            foreach (var point in points)
            {
                point.NumberOfPedestrians = 0;
            }

        }
        else
        {
            _events.Add(new Event()
            {
                Action = () => StartCrossTheRoad(points, trafficLights, EndTime + 1),
                Duration = 1,
                EndTime = EndTime + 1,
                StartTime = EndTime,
                Type = "Pedestrian"
            });
        }
    }

    public void CrossTheRoad(List<Point> points)
    {
        foreach (var point in points)
        {
            if (point.PedestriansOnTheRoad < 1.0d)
            {
                return;
            }
            point.PedestriansOnTheRoad--;
        }
       
    }

}
