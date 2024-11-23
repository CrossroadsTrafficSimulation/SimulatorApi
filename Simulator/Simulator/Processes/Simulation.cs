using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;
using System.Diagnostics;

namespace Simulator.Processes;

public class Simulation
{
    private readonly int _simulationTime = 24 * 60 * 60;
    public SimulationModel SimulationModel { get; set; } = null!;

    private readonly Statistics _statistics = new(24);
    private readonly List<Event>[] _statisticsQueue;

    private readonly List<Event>[] _trafficLightQueue;
    private readonly List<Event>[] _pedestrianQueue;
    private readonly List<Event>[] _vehicleQueue;
    private readonly Random _random = new();


    public Stopwatch PedestrianWatch { get; private set; } = new Stopwatch();
    public Stopwatch EdgeWatch { get; private set; } = new Stopwatch();

    public Simulation(int simulationTime)
    {
        _simulationTime = simulationTime;

        _trafficLightQueue = new List<Event>[_simulationTime];
        _pedestrianQueue = new List<Event>[_simulationTime];
        _vehicleQueue = new List<Event>[_simulationTime];

        _statisticsQueue = new List<Event>[_simulationTime];

        for (int i = 0; i < _simulationTime; i++)
        {
            _statisticsQueue[i] = [];
            _trafficLightQueue[i] = [];
            _pedestrianQueue[i] = [];
            _vehicleQueue[i] = [];
        }
    }

    public void ProcessStatistics(int currentTime)
    {
        _statisticsQueue[currentTime].ForEach(e => e.Action());
        _statisticsQueue[currentTime].Clear();
    }

    public void ProcessVehicles(int currentTime)
    {
        //Console.WriteLine($"Current time: {currentTime}");
        //Console.WriteLine($"Vehicle tasks: {_vehicleQueue[currentTime].Count}");

        /*        Console.WriteLine();
                Console.WriteLine($"VehicleMove: {_vehicleQueue[currentTime].Where(e => e.Type == EventType.VehicleMove).Count()}");
                Console.WriteLine($"VehicleMoveToNextEdge: {_vehicleQueue[currentTime].Where(e => e.Type == EventType.VehicleMoveToNextEdge).Count()}");
                Console.WriteLine($"VehicleGenerate: {_vehicleQueue[currentTime].Where(e => e.Type == EventType.VehicleGenerate).Count()}");
                Console.WriteLine($"VehicleAddGenerated: {_vehicleQueue[currentTime].Where(e => e.Type == EventType.VehicleAddGenerated).Count()}");
                Console.WriteLine();*/
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

    #region Setup
    public void SetUpFirstEvents()
    {
        const int InitialSimulationTime = 1;

        foreach (var trafficLight in SimulationModel.TrafficLights)
        {
            var switchTime = trafficLight.States[trafficLight.CurrentState];

            if (switchTime >= _simulationTime)
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

        _statistics.CreateAllCrosswalkStatistics(pedestrianFlowsList);
        foreach (var points in pedestrianFlowsList)
        {
            var generatePedestrian = CreateEvent(0, InitialSimulationTime, EventType.PedestrianGenerate,
                () => GeneratePedestrian(points, eventStartsAt: InitialSimulationTime));
            _pedestrianQueue[InitialSimulationTime].Add(generatePedestrian);
            var collectCrosswalkStatistics = CreateEvent(0, InitialSimulationTime, EventType.CollectStatistics,
                () => CollectCrossWalkStatistics(points, InitialSimulationTime));
            _statisticsQueue[InitialSimulationTime].Add(collectCrosswalkStatistics);
        }

        foreach (var flow in SimulationModel.Flows)
        {
            var generateVehicles = CreateEvent(0, InitialSimulationTime, EventType.VehicleGenerate,
                () => GenerateVehicles(flow, eventStartsAt: InitialSimulationTime));
            _vehicleQueue[InitialSimulationTime].Add(generateVehicles);
        }
        _statistics.CreateAllEdgeStatistics(SimulationModel.Edges);

        var collectEdgeStatistics = CreateEvent(0, InitialSimulationTime, EventType.CollectStatistics,
            () => CollectEdgeStatistics(InitialSimulationTime));
        _statisticsQueue[InitialSimulationTime].Add(collectEdgeStatistics);

    }
    #endregion

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
        return new SimulationResponseTo(_statistics);
    }

    #region Statistics
    public void CollectEdgeStatistics(int actionStartTime)
    {
        EdgeWatch.Start();
        var actionStartHour = (int)double.Floor(actionStartTime / (60.0 * 60.0));
        //foreach (var edgeStatistics in _statistics.EdgesVehicleDencity)
        //{
        //    var edge = SimulationModel
        //                .Edges
        //                .FirstOrDefault(e => e.Id == edgeStatistics.EdgeId);
        //    edgeStatistics.Statistics[actionStartHour] += (double)edge!.Vehicles.Count / (60 * 60);
        //}
        foreach (var edgeStat in _statistics.FastEdgeStatistics)
        {
            edgeStat.Item2.Statistics[actionStartHour] += (double)edgeStat.Item1.Vehicles.Count / (60 * 60);
        }
        var nextEndTime = actionStartTime + 1;
        if (nextEndTime >= _simulationTime)
        {
            return;
        }
        _statisticsQueue[nextEndTime].Add(CreateEvent(actionStartTime, nextEndTime, EventType.CollectStatistics,
            () => CollectEdgeStatistics(nextEndTime)));
        EdgeWatch.Stop();
    }

    public void CollectCrossWalkStatistics(List<Point> crosswalk, int actionStartTime)
    {
        PedestrianWatch.Start();
        var actionStartHour = (int)double.Floor(actionStartTime / (60.0 * 60.0));
        var ids = crosswalk.Select(c => c.Id);
        var crosswalkStatistics = _statistics.CrossWalkPedestrianDencity.FirstOrDefault(c => c.Croswalk.All(c => ids.Contains(c)));
        crosswalkStatistics!.Statistics[actionStartHour] += (double)crosswalk[0].PedestriansQueueCount / (60 * 60);
        var nextEndTime = actionStartTime + 1;
        if (nextEndTime >= _simulationTime)
        {
            return;
        }
        _statisticsQueue[nextEndTime].Add(CreateEvent(actionStartTime, nextEndTime, EventType.CollectStatistics,
            () => CollectCrossWalkStatistics(crosswalk, nextEndTime)));
        PedestrianWatch.Stop();
    }
    #endregion

    #region Traffic lights
    private void ChangeTrafficLightState(TrafficLight trafficLight, int eventStartsAt)
    {
        trafficLight.SwitchState();

        var eventEndsAt = eventStartsAt + trafficLight.States[trafficLight.CurrentState];

        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        var changeTafficLightState = CreateEvent(eventStartsAt, eventEndsAt, EventType.TrafficLightSwitch,
            () => ChangeTrafficLightState(trafficLight, eventEndsAt));
        _trafficLightQueue[eventEndsAt].Add(changeTafficLightState);
    }
    #endregion

    #region Pedestrians
    public void GeneratePedestrian(List<Point> crosswalk, int eventStartsAt)
    {
        var eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        var generatePedestrian = CreateEvent(eventStartsAt, eventEndsAt, EventType.PedestrianGenerate,
            () => GeneratePedestrian(crosswalk, eventStartsAt + 1));
        _pedestrianQueue[eventEndsAt].Add(generatePedestrian);

        var flow = crosswalk
            .FirstOrDefault()?.PedestriansFlow?
            .Where(p => (p.Key.Hour * 60 * 60) + (p.Key.Minute * 60) + p.Key.Second <= eventStartsAt)
            .Select(p => p.Value)
            .LastOrDefault();

        if (flow is null || flow.Value == 0)
        {
            return;
        }

        foreach (var point in crosswalk)
        {
            point.PedestriansQueueCount += flow.Value / 60.0;
        }

        var trafficLights = crosswalk.SelectMany(p => p.TrafficLights).ToList();

        StartCrossTheRoad(crosswalk, trafficLights, eventStartsAt);
    }

    public void StartCrossTheRoad(List<Point> crosswalk, List<TrafficLight> trafficLights, int eventStartsAt)
    {
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            var tempPedestrians = (int)crosswalk.First().PedestriansQueueCount;
            foreach (var point in crosswalk)
            {
                point.PedestriansOnTheRoadCount += (int)point.PedestriansQueueCount;
                point.PedestriansQueueCount -= (int)point.PedestriansQueueCount;
            }

            for (int i = 0; i < tempPedestrians; i++)
            {
                var crossingTime = _random.Next(5, 10) * crosswalk.Count;
                var endOfNext = eventStartsAt + crossingTime;
                if (endOfNext < _simulationTime)
                {
                    var cross = CreateEvent(eventStartsAt, endOfNext, EventType.PedestrianCross, () => CrossTheRoad(crosswalk));
                    _pedestrianQueue[endOfNext].Add(cross);
                }
            }
        }
        else
        {
            //var secondsTillRed = trafficLights.FirstOrDefault()?.SecondsTillRed(eventStartsAt) ?? 0;
            //eventStartsAt += secondsTillRed;
            var eventEndsAt = eventStartsAt + 1;

            if (eventEndsAt < _simulationTime)
            {
                var startCross = CreateEvent(eventStartsAt, eventEndsAt, EventType.PedestrianStartCross,
                    () => StartCrossTheRoad(crosswalk, trafficLights, eventEndsAt));
                _pedestrianQueue[eventEndsAt].Add(startCross);
            }
        }
    }

    public void CrossTheRoad(List<Point> crosswalk)
    {
        foreach (var point in crosswalk)
        {
            if (point.PedestriansOnTheRoadCount < 1.0d)
            {
                return;
            }

            point.PedestriansOnTheRoadCount--;
        }
    }
    #endregion

    #region Vehicles
    public void GenerateVehicles(Flow flow, int eventStartsAt)
    {
        var eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        var generate = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleGenerate, () => GenerateVehicles(flow, eventEndsAt));
        _vehicleQueue[eventEndsAt].Add(generate);

        var vehiclesPerMinute = flow.Density
           .Where(p => (p.Key.Hour * 60 * 60) + (p.Key.Minute * 60) + p.Key.Second <= eventStartsAt)
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
            AddGeneratedToEdge(flow, spawnEdge, eventStartsAt, carSize);
        }
    }

    private void AddGeneratedToEdge(Flow flow, Edge spawnEdge, int eventStartsAt, double carSize)
    {
        int eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        var possibleRoutes = SimulationModel.Routes
            .Where(route => route[0].Id == flow.PointId)
            .ToList();
        var vehiclesAddedToEdgeCount = 0;

        for (int i = 0; i < (int)flow.VehiclesInQueue; i++)
        {
            var randomRouteIndex = _random.Next(0, possibleRoutes.Count);
            var route = possibleRoutes[randomRouteIndex];
            var vehicle = new Vehicle()
            {
                Route = route,
                Size = carSize
            };

            if (spawnEdge.TryEnqueueVehicle(vehicle))
            {
                // Add time here to simulate the delay from the queue
                MoveVehicle(vehicle, eventStartsAt);
                vehiclesAddedToEdgeCount++;
            }
        }

        flow.VehiclesInQueue -= vehiclesAddedToEdgeCount;
    }

    public void MoveVehicle(Vehicle vehicle, int eventStartsAt)
    {
        int eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        if (vehicle.TryDriveThrough())
        {
            eventEndsAt = eventStartsAt + (int)(vehicle.CurrentEdge!.Distance / vehicle.CurrentEdge!.SpeedLimit);

            if (eventEndsAt < _simulationTime)
            {
                var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                    () => ToNextEdge(vehicle.CurrentEdge, eventEndsAt));
                _vehicleQueue[eventEndsAt].Add(enqueue);
            }
        }
        // Red traffic light or pedestrians
        else
        {
            // Traffic light prediction
            /*            var secondsTillGreen = vehicle.CurrentEdge!.TrafficLight?.SecondsTillGreen(eventStartsAt) ?? 0;
                        eventStartsAt += secondsTillGreen;
                        eventEndsAt += secondsTillGreen;*/

            ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle);
        }
    }

    public void ToNextEdge(Edge currEdge, int eventStartsAt)
    {
        var eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        if (currEdge.Vehicles.TryPeek(out var vehicle))
        {
            if (vehicle.IsLastPoint)
            {
                // PROCESS LAST POINT
                // Points: A => B => C => D
                // Edges: AB => BC => CD
                // Traffic light in D point won't work
                var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                    () =>
                    {
                        if (vehicle.CurrentEdge!.TryDequeueVehicle(out var dequeue))
                        {
                            //Console.WriteLine($"Moved away from {vehicle.CurrentEdge.Id}");
                        }
                    });
                _vehicleQueue[eventEndsAt].Add(enqueue);
            }
            else
            {
                if (vehicle.NextEdge!.TryEnqueueVehicle(vehicle))
                {
                    if (vehicle.CurrentEdge!.TryDequeueVehicle(out var _))
                    {
                        vehicle.CurrentRoutePos++;
                        ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle);
                    }
                }
                else
                {
                    var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                        () => ToNextEdge(vehicle.CurrentEdge!, eventEndsAt));
                    _vehicleQueue[eventEndsAt].Add(enqueue);
                }
            }
        }
    }

    private void ScheduleMoveEvent(int currentTime, int eventEndsTime, Vehicle vehicle)
    {
        if (eventEndsTime < _simulationTime)
        {
            var move = CreateEvent(currentTime, eventEndsTime, EventType.VehicleMove,
                () => MoveVehicle(vehicle, eventEndsTime));
            _vehicleQueue[eventEndsTime].Add(move);
        }
    }
    #endregion
}
