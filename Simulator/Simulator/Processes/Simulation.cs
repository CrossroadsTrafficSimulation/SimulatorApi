﻿using Simulator.Model;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Response;
using Simulator.Model.Entites;
using Simulator.Model.Enums;

namespace Simulator.Processes;

public class Simulation
{
    private const int SimulationTime = 2 * 60 * 60;
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

        if (eventEndsAt >= SimulationTime)
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
        if (eventEndsAt >= SimulationTime)
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
                if (endOfNext < SimulationTime)
                {
                    var cross = CreateEvent(eventStartsAt, endOfNext, EventType.PedestrianCross, () => CrossTheRoad(crosswalk));
                    _pedestrianQueue[endOfNext].Add(cross);
                }
            }
        }
        else
        {
            var eventEndsAt = eventStartsAt + 1;
            if (eventEndsAt < SimulationTime)
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
        if (eventEndsAt >= SimulationTime)
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
        if (eventEndsAt >= SimulationTime)
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
            var route = SimulationModel.Routes[randomRouteIndex];
            var vehicle = new Vehicle()
            {
                CurrentEdgeId = spawnEdge.Id,
                Route = route,
                Size = carSize
            };

            if (spawnEdge.TryEnqueueVehicle(vehicle))
            {
                // Add time here to simulate the delay from the queue
                MoveVehicle(vehicle.CurrentEdge!, vehicle.NextEdge!, eventStartsAt);
                //ScheduleMoveEvent(eventStartsAt, eventEndsAt, vehicle.CurrentEdge!, vehicle.NextEdge!);
                vehiclesAddedToEdgeCount++;
            }
            else
            {
                var addGenetared = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleAddGenerated,
                    () => AddGeneratedToEdge(flow, spawnEdge, eventEndsAt, carSize));
                _vehicleQueue[eventEndsAt].Add(addGenetared);
            }
        }

        flow.VehiclesInQueue -= vehiclesAddedToEdgeCount;
    }

    public void MoveVehicle(Edge sourceEdge, Edge destEdge, int eventStartsAt)
    {
        int eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= SimulationTime)
        {
            return;
        }

        if (sourceEdge.Vehicles.TryDequeue(out var vehicle))
        {
            if (vehicle.TryDriveThrough())
            {
                eventEndsAt = eventStartsAt + (int)Math.Ceiling(sourceEdge.Distance / sourceEdge.SpeedLimit);

                if (eventEndsAt < SimulationTime)
                {
                    // Add time here to simulate the delay from the queue
                    var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                        () => ToNextEdge(vehicle, sourceEdge, destEdge, eventEndsAt));
                    _vehicleQueue[eventEndsAt].Add(enqueue);
                }
            }
            // Red traffic light or pedestrians
            // If last point, then vehicle is deleted
            else if (!vehicle.IsLastPoint)
            {
                ScheduleMoveEvent(eventStartsAt, eventEndsAt, sourceEdge, destEdge);
            }
        }
        else
        {
            ScheduleMoveEvent(eventStartsAt, eventEndsAt, sourceEdge, destEdge);
        }
    }

    public void ToNextEdge(Vehicle vehicle, Edge currEdge, Edge nextEdge, int eventStartsAt)
    {
        var eventEndsAt = eventStartsAt + 1;
        if (eventEndsAt >= SimulationTime)
        {
            return;
        }

        if (nextEdge.TryEnqueueVehicle(vehicle))
        {
            // Add time here to simulate the delay from the queue
            _ = currEdge.DequeueVehicle();
            ScheduleMoveEvent(eventStartsAt, eventEndsAt, currEdge, nextEdge);
        }
        else
        {
            var enqueue = CreateEvent(eventStartsAt, eventEndsAt, EventType.VehicleMoveToNextEdge,
                () => ToNextEdge(vehicle, currEdge, nextEdge, eventEndsAt));
            _vehicleQueue[eventEndsAt].Add(enqueue);
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
