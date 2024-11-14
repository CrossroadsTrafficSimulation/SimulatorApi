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
    private const int simulationTime = 24*60*60;
    private readonly List<Event> _events = [];
    private readonly List<Event>[] _trafficLightQueue = new List<Event>[simulationTime];
    private readonly List<Event>[] _pedestrianQueue = new List<Event>[simulationTime];
    private readonly List<Event>[] _vehicleQueue = new List<Event>[simulationTime];

    public Simulation()
    {
        for (int i = 0; i < simulationTime; i++)
        {
            _trafficLightQueue[i] = new List<Event>();
            _pedestrianQueue[i] = new List<Event>();
            _vehicleQueue[i] = new List<Event>();
        }
    }
    public void ProcessVehicles(int currentTime)
    {
        var vehicleEvents = _events.Where(e => e.Type.Equals("Vehicle", StringComparison.OrdinalIgnoreCase));
        throw new NotImplementedException();
    }
    public void ProcessTrafficLights(int currentTime)
    {
        var trafficLightsEvents = _trafficLightQueue[currentTime];
        foreach (var e in trafficLightsEvents)
        {
            e.Action();
        }
        _trafficLightQueue[currentTime].Clear();


    }

    public void ProcessPedestrians(int currentTime)
    {
        var pedestriansEvents = _pedestrianQueue[currentTime];
        foreach (var e in pedestriansEvents)
        {
            e.Action();
        }
        _pedestrianQueue[currentTime].Clear();

        //Console.WriteLine();
        //Console.WriteLine($"Current time: {currentTime}");
        //foreach (var p in SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null))
        //{
        //    if (p.Value.PedestriansOnTheRoad != 0.0 ||p.Value.NumberOfPedestrians != 0.0)
        //        Console.WriteLine($"Pedestrians: {p.Key} - {p.Value.PedestriansOnTheRoad} - {p.Value.NumberOfPedestrians}");
        //}
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
        var endOfNext = endOfAction + trafficLight.States[trafficLight.CurrentState];
        if (endOfNext >= simulationTime)
            return;
        _trafficLightQueue[endOfNext].Add(new Event()
        {
            Action = () => ChangeTrafficLightState(trafficLight, endOfNext),
            Duration = trafficLight.States[trafficLight.CurrentState],
            EndTime = endOfNext,
            StartTime = endOfAction,
            Type = "TrafficLight"
        });
    }



    public void SetUpFirstEvents()
    {
        foreach (var trafficLight in SimulationModel.TrafficLights)
        {
            var endOfNext = trafficLight.States[trafficLight.CurrentState];
            if (endOfNext >= simulationTime)
                return;
            _trafficLightQueue[endOfNext].Add(new Event()
            {
                Action = () => ChangeTrafficLightState(trafficLight, endOfNext),
                Duration = trafficLight.States[trafficLight.CurrentState],
                EndTime = endOfNext,
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

            _pedestrianQueue[1].Add(new Event()
            {
                Action = () => GeneratePedestrian(points, endTime: 1),
                Duration = 1,
                StartTime = 0,
                EndTime = 1,
                Type = "Pedestrian"
            });
        }
    }

    public void GeneratePedestrian(List<Point> crosswalk, int endTime)
    {
        var endOfNext = endTime + 1;
        if (endOfNext >= simulationTime)
            return;
        _pedestrianQueue[endOfNext].Add(new Event()
        {
            Action = () => GeneratePedestrian(crosswalk, endTime + 1),
            Duration = 1,
            StartTime = endTime,
            EndTime = endTime + 1,
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
                point.NumberOfPedestrians += flow.Value/60.0;
            }

            if (crosswalk[0].NumberOfPedestrians >= 1.0)
            {
                foreach (var point in crosswalk)
                {
                    point.PedestriansOnTheRoad += (int)(point.NumberOfPedestrians);
                    point.NumberOfPedestrians = point.NumberOfPedestrians - (int)point.NumberOfPedestrians;
                }
            }
            
            for (int i = 0; i < crosswalk[0].PedestriansOnTheRoad; i++)
            {
                var crossingTime = rand.Next(5, 10) * crosswalk.Count;
                endOfNext = endTime + crossingTime;
                if (endOfNext >= simulationTime)
                    return;
                _pedestrianQueue[endOfNext].Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = endOfNext,
                    StartTime = endTime,
                    Type = "Pedestrian"
                });
            }
        }
        else
        {
            foreach (var point in crosswalk)
            {
                point.NumberOfPedestrians += flow.Value/60.0;
            }
            endOfNext = endTime + 1;
            if (endOfNext >= simulationTime)
                return;
            _pedestrianQueue[endOfNext].Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, endOfNext),
                Duration = 1,
                EndTime = endOfNext,
                StartTime = endTime,
                Type = "Pedestrian"
            });
        }
        //Console.WriteLine();

       
        //Console.WriteLine("")
    }

    public void StartCrossTheRoad(List<Point> crosswalk, List<TrafficLight> trafficLights, int EndTime)
    {
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            foreach (var point in crosswalk)
            {
                point.PedestriansOnTheRoad += (int)point.NumberOfPedestrians;
            }

            var rand = new Random();

            for (int i = 0; i < crosswalk.FirstOrDefault()?.NumberOfPedestrians; i++)
            {
                var crossingTime = rand.Next(5, 10) * crosswalk.Count;
                var endOfNext = EndTime + crossingTime;
                if (endOfNext >= simulationTime)
                    return;
                _pedestrianQueue[endOfNext].Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = endOfNext,
                    StartTime = EndTime,
                    Type = "Pedestrian"
                });
            }
            foreach (var point in crosswalk)
            {
                point.NumberOfPedestrians = point.NumberOfPedestrians - (int)point.NumberOfPedestrians;
            }

        }
        else
        {
            var endOfNext = EndTime + 1;
            if (endOfNext >= simulationTime)
                return;
            _pedestrianQueue[endOfNext].Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, endOfNext),
                Duration = 1,
                EndTime = endOfNext,
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
