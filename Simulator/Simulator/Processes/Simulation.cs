using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;

namespace Simulator.Processes;

public class Simulation
{
    private readonly int _simulationTime = 24 * 60 * 60;
    public SimulationModel SimulationModel { get; set; } = null!;
    private readonly List<Event>[] _trafficLightQueue;
    private readonly List<Event>[] _pedestrianQueue;
    private readonly List<Event>[] _vehicleQueue;
    private readonly Random _random = new();

    public Simulation(int simulationTime)
    {
        _simulationTime = simulationTime;

        _trafficLightQueue = new List<Event>[_simulationTime];
        _pedestrianQueue = new List<Event>[_simulationTime];
        _vehicleQueue = new List<Event>[_simulationTime];

        for (int i = 0; i < _simulationTime; i++)
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
        //Console.WriteLine($"Current time: {currentTime}");
        //foreach (var edge in SimulationModel.Edges)
        //{
        //    if (edge.Vehicles.Count != 0)
        //    Console.WriteLine($"Edge id: {edge.Id} contains {edge.Vehicles.Count} vehicles with car length: {edge.SumCarsLength}");
        //}
        //foreach (var flow in SimulationModel.Flows)
        //{
        //    Console.WriteLine($"Flow id: {flow.PointId} contains {flow.VehiclesInQueue} vehicles");
        //}
        //Console.WriteLine();
    }

    public void ProcessTrafficLights(int currentTime)
    {
        _trafficLightQueue[currentTime].ForEach(e => e.Action());
        _trafficLightQueue[currentTime].Clear();
    }

    public void ProcessPedestrians(int currentTime)
    {
        /*        Console.WriteLine();
                Console.WriteLine($"Current time: {currentTime}");
                var vehEvents = _vehicleQueue[currentTime].GroupBy(e => e.Type).ToList();
                foreach (var vehEvent in vehEvents)
                {
                    Console.WriteLine($"Type: {vehEvent.Key}, quantity: {vehEvent.Count()}");
                }*/
        //Console.WriteLine($"Number of vehicle events: {_vehicleQueue[currentTime].Count}");

        _pedestrianQueue[currentTime].ForEach(e => e.Action());
        _pedestrianQueue[currentTime].Clear();

        /*        foreach (var p in SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null))
                {
                    if (p.Value.PedestriansOnTheRoadCount != 0.0 || p.Value.PedestriansQueueCount != 0.0)
                        Console.WriteLine($"Pedestrians: {p.Key} - {p.Value.PedestriansOnTheRoadCount} - {p.Value.PedestriansQueueCount}");
                }*/

    }

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

        foreach (var points in pedestrianFlowsList)
        {
            var generatePedestrian = CreateEvent(0, InitialSimulationTime, EventType.PedestrianGenerate,
                () => GeneratePedestrian(points, eventStartsAt: InitialSimulationTime));
            _pedestrianQueue[InitialSimulationTime].Add(generatePedestrian);
        }

        foreach (var flow in SimulationModel.Flows)
        {
            var generateVehicles = CreateEvent(0, InitialSimulationTime, EventType.VehicleGenerate,
                () => GenerateVehicles(flow, eventStartsAt: InitialSimulationTime));
            _vehicleQueue[InitialSimulationTime].Add(generateVehicles);
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

        //Console.WriteLine($"Vehicles in queue: {flow.VehiclesInQueue:F3}");
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
                CurrentEdgeId = spawnEdge.Id,
                Route = route,
                Size = carSize
            };

            if (spawnEdge.TryEnqueueVehicle(vehicle))
            {
                //Console.WriteLine($"Enqueued {spawnEdge.Vehicles.Count} to {spawnEdge.Id}");
                // Add time here to simulate the delay from the queue
                MoveVehicle(vehicle, eventStartsAt);
                //ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle.CurrentEdge!, vehicle.NextEdge!);
                vehiclesAddedToEdgeCount++;
            }
        }

        if (vehiclesAddedToEdgeCount < (int)flow.VehiclesInQueue)
        {
            var addGenetared = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleAddGenerated,
                () => AddGeneratedToEdge(flow, spawnEdge, eventEndsAt, carSize));
            _vehicleQueue[eventEndsAt].Add(addGenetared);
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
/*            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Moving to {vehicle!.CurrentEdge!.StartPoint.Id}");
            Console.ForegroundColor = originalColor;*/

            eventEndsAt = eventStartsAt + (int)Math.Ceiling(vehicle.CurrentEdge!.Distance / vehicle.CurrentEdge!.SpeedLimit);

            if (eventEndsAt < _simulationTime)
            {
                if (!vehicle.IsLastPoint)
                {
                    var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                        () => ToNextEdge(vehicle.CurrentEdge, vehicle.NextEdge!, eventEndsAt));
                    _vehicleQueue[eventEndsAt].Add(enqueue);
                }
                else
                {
                    // PROCESS LAST POINT
                    // Points: A => B => C => D
                    // Edges: AB => BC => CD
                    // Traffic light in D point won't work
                    //Console.WriteLine($"____!!!Is in last point");
                    var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                        () =>
                        {
                            if (vehicle.CurrentEdge.TryDequeueVehicle(out var dequeue))
                            {
                                //Console.WriteLine($"Moved away from {vehicle.CurrentEdge.Id}");
                            }
                        });
                    _vehicleQueue[eventEndsAt].Add(enqueue);
                }
            }
        }
        // Red traffic light or pedestrians
        else
        {
            ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle);
        }
    }

    public void ToNextEdge(Edge currEdge, Edge nextEdge, int eventStartsAt)
    {
        //var originalColor = Console.ForegroundColor;
        //Console.ForegroundColor = ConsoleColor.Blue;

        //Console.WriteLine($"Trying to switch edge from to {currEdge.Id} to {nextEdge.Id}");

        var eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= _simulationTime)
        {
            return;
        }

        //Console.WriteLine($"Trying to dequeue from {currEdge.Id}");
        if (currEdge.TryDequeueVehicle(out var vehicle))
        {
            //Console.WriteLine($"Deque");
            vehicle!.CurrentRoutePos++;
            // Add time here to simulate the delay from the queue
            _ = nextEdge.TryEnqueueVehicle(vehicle!);
            ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle!);
        }
        else
        {
            //Console.WriteLine($"FAIL: Deque");
            var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                () => ToNextEdge(currEdge, nextEdge, eventEndsAt));
            _vehicleQueue[eventEndsAt].Add(enqueue);
        }

        //Console.ForegroundColor = originalColor;
    }

    private void ScheduleMoveEvent(int currentTime, int eventEndsTime, Vehicle vehicle)
    {
        var move = CreateEvent(currentTime, eventEndsTime, EventType.VehicleMove,
            () => MoveVehicle(vehicle, eventEndsTime));
        _vehicleQueue[eventEndsTime].Add(move);
    }
    #endregion
}
