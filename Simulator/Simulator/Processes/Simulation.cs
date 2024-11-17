using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;

namespace Simulator.Processes;

public class Simulation
{
    private const int SimulationTime = 24 * 60 * 60;
    public SimulationModel SimulationModel { get; set; } = null!;
    private readonly List<Event>[] _trafficLightQueue = new List<Event>[SimulationTime];
    private readonly List<Event>[] _pedestrianQueue = new List<Event>[SimulationTime];
    private readonly List<Event>[] _vehicleQueue = new List<Event>[SimulationTime];
    private readonly Random _random = new();

    public Simulation()
    {
        for (int i = 0; i < SimulationTime; i++)
        {
            _trafficLightQueue[i] = [];
            _pedestrianQueue[i] = [];
            _vehicleQueue[i] = [];
        }
    }

    public void ProcessVehicles(int currentTime)
    {
        _vehicleQueue[currentTime].ForEach(e => e.Action());
        _vehicleQueue[currentTime].Clear();
    }

    public void ProcessTrafficLights(int currentTime)
    {
        _trafficLightQueue[currentTime].ForEach(e => e.Action());
        _trafficLightQueue[currentTime].Clear();
    }

    public void ProcessPedestrians(int currentTime)
    {
        _pedestrianQueue[currentTime].ForEach(e => e.Action());
        _pedestrianQueue[currentTime].Clear();
    }

    public void SetUpFirstEvents()
    {
        const int InitialSimulationTime = 1;

        foreach (var trafficLight in SimulationModel.TrafficLights)
        {
            var switchTime = trafficLight.States[trafficLight.CurrentState];

            if (switchTime >= SimulationTime)
            {
                return;
            }

            _trafficLightQueue[switchTime].Add(new Event()
            {
                Action = () => ChangeTrafficLightState(trafficLight, switchTime),
                Duration = trafficLight.States[trafficLight.CurrentState],
                EndTime = switchTime,
                StartTime = 0,
                Type = EventType.TrafficLightSwitch
            });
        }

        var pedestrianFlowsList = SimulationModel.Points
            .Where(p => p.Value.PedestriansFlow is not null)
            .GroupBy(p => p.Value.PedestriansFlow)
            .Select(group => group.Select(p => p.Value).ToList())
            .ToList();

        foreach (var points in pedestrianFlowsList)
        {
            _pedestrianQueue[InitialSimulationTime].Add(new Event()
            {
                Action = () => GeneratePedestrian(points, currentTime: InitialSimulationTime),
                Duration = 1,
                StartTime = 0,
                EndTime = InitialSimulationTime,
                Type = EventType.PedestrianGenerate
            });
        }

        foreach (var flow in SimulationModel.Flows)
        {
            _vehicleQueue[InitialSimulationTime].Add(new Event()
            {
                Action = () => GenerateVehicles(flow, currentTime: InitialSimulationTime),
                Duration = 1,
                StartTime = 0,
                EndTime = InitialSimulationTime,
                Type = EventType.VehicleGenerate
            });
        }
    }

    private static Event CreateEvent(int startTime, int endTime, EventType type, Action action)
    {
        return new Event()
        {
            Action = action,
            StartTime = startTime,
            EndTime = endTime,
            Duration = endTime - startTime,
            Type = type
        };
    }

    public SimulationResponseTo GetResult()
    {
        throw new NotImplementedException();
    }

    #region Traffic lights
    private void ChangeTrafficLightState(TrafficLight trafficLight, int currentTime)
    {
        trafficLight.SwitchState();
        var switchTime = currentTime + trafficLight.States[trafficLight.CurrentState];

        if (switchTime >= SimulationTime)
        {
            return;
        }

        _trafficLightQueue[switchTime].Add(new Event()
        {
            Action = () => ChangeTrafficLightState(trafficLight, switchTime),
            Duration = trafficLight.States[trafficLight.CurrentState],
            EndTime = switchTime,
            StartTime = currentTime,
            Type = EventType.TrafficLightSwitch
        });
    }
    #endregion

    #region Pedestrians
    public void GeneratePedestrian(List<Point> crosswalk, int currentTime)
    {
        var eventEndsTime = currentTime + 1;
        if (eventEndsTime >= SimulationTime)
        {
            return;
        }

        var generate = CreateEvent(currentTime, eventEndsTime, EventType.PedestrianGenerate, () => GeneratePedestrian(crosswalk, currentTime + 1));
        _pedestrianQueue[eventEndsTime].Add(generate);

        var flow = crosswalk
            .FirstOrDefault()?.PedestriansFlow?
            .Where(p => (p.Key.Hour * 60 * 60) + (p.Key.Minute * 60) + p.Key.Second <= currentTime)
            .Select(p => p.Value)
            .LastOrDefault();

        if (flow is null || flow.Value == 0)
        {
            return;
        }

        var trafficLights = crosswalk.SelectMany(p => p.TrafficLights).ToList();
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        foreach (var point in crosswalk)
        {
            point.PedestriansQueueCount += flow.Value / 60.0;
        }

        if (isPossibleToCross)
        {
            if (crosswalk[0].PedestriansQueueCount >= 1.0)
            {
                foreach (var point in crosswalk)
                {
                    point.CrossingPedestriansCount += (int)point.PedestriansQueueCount;
                    point.PedestriansQueueCount -= (int)point.PedestriansQueueCount;
                }
            }

            for (int i = 0; i < crosswalk.FirstOrDefault()?.PedestriansQueueCount; i++)
            {
                var crossingTime = _random.Next(5, 10) * crosswalk.Count;
                eventEndsTime = currentTime + crossingTime;
                
                if (eventEndsTime >= SimulationTime)
                {
                    return;
                }

                _pedestrianQueue[eventEndsTime].Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = eventEndsTime,
                    StartTime = currentTime,
                    Type = EventType.PedestrianCross
                });
            }
        }
        else
        {
            eventEndsTime = currentTime + 1;
            if (eventEndsTime >= SimulationTime)
            {
                return;
            }
            _pedestrianQueue[eventEndsTime].Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, eventEndsTime),
                Duration = 1,
                EndTime = eventEndsTime,
                StartTime = currentTime,
                Type = EventType.PedestrianTryCross
            });
        }
    }

    public void StartCrossTheRoad(List<Point> crosswalk, List<TrafficLight> trafficLights, int EndTime)
    {
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            foreach (var point in crosswalk)
            {
                point.CrossingPedestriansCount += (int)point.PedestriansQueueCount;
            }

            for (int i = 0; i < crosswalk.FirstOrDefault()?.PedestriansQueueCount; i++)
            {
                var crossingTime = _random.Next(5, 10) * crosswalk.Count;
                var endOfNext = EndTime + crossingTime;

                if (endOfNext >= SimulationTime)
                {
                    return;
                }

                _pedestrianQueue[endOfNext].Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = endOfNext,
                    StartTime = EndTime,
                    Type = EventType.PedestrianCross
                });
            }

            foreach (var point in crosswalk)
            {
                point.PedestriansQueueCount -= (int)point.PedestriansQueueCount;
            }

        }
        else
        {
            var endOfNext = EndTime + 1;
            if (endOfNext >= SimulationTime)
                return;
            _pedestrianQueue[endOfNext].Add(new Event()
            {
                Action = () => StartCrossTheRoad(crosswalk, trafficLights, endOfNext),
                Duration = 1,
                EndTime = endOfNext,
                StartTime = EndTime,
                Type = EventType.PedestrianTryCross
            });
        }
    }

    public void CrossTheRoad(List<Point> crosswalk)
    {
        foreach (var point in crosswalk)
        {
            if (point.CrossingPedestriansCount < 1.0d)
            {
                return;
            }
            point.CrossingPedestriansCount--;
        }

    }
    #endregion

    #region Vehicles
    public void GenerateVehicles(Flow flow, int currentTime)
    {
        var eventEndsTime = currentTime + 1;

        if (eventEndsTime >= SimulationTime)
        {
            return;
        }

        var generate = CreateEvent(currentTime, eventEndsTime, EventType.VehicleGenerate, () => GenerateVehicles(flow, eventEndsTime));
        _vehicleQueue[eventEndsTime].Add(generate);

        var vehiclesPerMinute = flow.Density
           .Where(p => (p.Key.Hour * 60 * 60) + (p.Key.Minute * 60) + p.Key.Second <= currentTime)
           .Select(p => p.Value)
           .LastOrDefault();

        if (vehiclesPerMinute == 0)
        {
            return;
        }

        var spawnEdge = SimulationModel.Edges.First(e => e.StartPointId == flow.PointId);

        var carSize = double.Round(_random.NextDouble(), 1) + 4.0;
        flow.VehiclesInQueue += vehiclesPerMinute / 60.0;

        if (flow.VehiclesInQueue >= 1.0)
        {
            AddGenerated(flow, spawnEdge, currentTime, carSize);
        }
    }

    private void AddGenerated(Flow flow, Edge spawnEdge, int currentTime, double carSize)
    {
        int eventEndsTime = currentTime + 1;
        if (eventEndsTime >= SimulationTime)
        {
            return;
        }

        var possibleRoutes = SimulationModel.Routes
            .Where(route => route[0].Id == flow.PointId)
            .ToList();
        var vehiclesAddedToEdgeCount = 0;

        for (int i = 0; i < (int)flow.VehiclesInQueue; i++)
        {
            var randomRouteIndex = _random.Next(0, possibleRoutes.Count + 1);
            var route = SimulationModel.Routes[randomRouteIndex];
            var vehicle = new Vehicle()
            {
                CurrentEdgeId = spawnEdge.Id,
                Route = route,
                Size = carSize
            };

            if (spawnEdge.TryEnqueueVehicle(vehicle))
            {
                ScheduleMoveEvent(currentTime, eventEndsTime, vehicle.CurrentEdge!, vehicle.NextEdge!);
                vehiclesAddedToEdgeCount++;
            }
            else
            {
                var addGenetared = CreateEvent(currentTime, eventEndsTime, EventType.VehicleWaitGenerate, 
                    () => AddGenerated(flow, spawnEdge, eventEndsTime, carSize));
                _vehicleQueue[eventEndsTime].Add(addGenetared);
            }
        }

        flow.VehiclesInQueue -= vehiclesAddedToEdgeCount;
    }

    public void NextEdge(Vehicle vehicle, Edge currEdge, Edge nextEdge, int currentTime)
    {
        var eventEndsTime = currentTime + 1;

        if (eventEndsTime >= SimulationTime)
        {
            return;
        }

        if (nextEdge.TryEnqueueVehicle(vehicle))
        {
            _ = currEdge.DequeueVehicle();
            ScheduleMoveEvent(currentTime, eventEndsTime, currEdge, nextEdge);
        }
        else
        {
            var enqueue = CreateEvent(currentTime, eventEndsTime, EventType.VehicleNextEdge, 
                () => NextEdge(vehicle, currEdge, nextEdge, eventEndsTime));
            _vehicleQueue[eventEndsTime].Add(enqueue);
        }
    }

    public void MoveVehicle(Edge sourceEdge, Edge destEdge, int currentTime)
    {
        int eventEndsTime = currentTime + 1;
        if (eventEndsTime >= SimulationTime)
        {
            return;
        }

        if (sourceEdge.Vehicles.TryDequeue(out var vehicle))
        {
            if (vehicle.TryDriveThrough())
            {
                eventEndsTime = currentTime + (int)Math.Ceiling(sourceEdge.Distance / sourceEdge.SpeedLimit);

                if (eventEndsTime < SimulationTime)
                {
                    var enqueue = CreateEvent(currentTime, eventEndsTime, EventType.VehicleToNext,
                        () => NextEdge(vehicle, sourceEdge, destEdge, eventEndsTime));
                    _vehicleQueue[eventEndsTime].Add(enqueue);
                }
            }
            else if (!vehicle.IsLastPoint)
            {
                ScheduleMoveEvent(currentTime, eventEndsTime, sourceEdge, destEdge);
            }
        }
        else
        {
            ScheduleMoveEvent(currentTime, eventEndsTime, sourceEdge, destEdge);
        }
    }

    private void ScheduleMoveEvent(int currentTime, int eventEndsTime, Edge sourceEdge, Edge destEdge)
    {
        var move = CreateEvent(currentTime, eventEndsTime, EventType.VehicleMove,
            () => MoveVehicle(sourceEdge, destEdge, eventEndsTime));
        _vehicleQueue[eventEndsTime].Add(move);
    }
    #endregion
}
