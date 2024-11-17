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
    private const int simulationTime = 24 * 60 * 60;
    private readonly List<Event>[] _trafficLightQueue = new List<Event>[simulationTime];
    private readonly List<Event>[] _pedestrianQueue = new List<Event>[simulationTime];
    private readonly List<Event>[] _vehicleQueue = new List<Event>[simulationTime];
    private readonly Random _random = new Random();

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
        var vehicleEvents = _vehicleQueue[currentTime];
        foreach (var e in vehicleEvents)
        {
            e.Action();
        }
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

        Console.WriteLine();
        Console.WriteLine($"Current time: {currentTime}");
        foreach (var p in SimulationModel.Points.Where(p => p.Value.PedestriansFlow is not null))
        {
            if (p.Value.PedestriansOnTheRoad != 0.0 || p.Value.NumberOfPedestrians != 0.0)
                Console.WriteLine($"Pedestrians: {p.Key} - {p.Value.PedestriansOnTheRoad} - {p.Value.NumberOfPedestrians}");
        }
        //foreach (var )
    }

    public SimulationResponseTo GetResult()
    {
        throw new NotImplementedException();
    }

    #region Traffic lights
    private void ChangeTrafficLightState(TrafficLight? trafficLight, int endTime)
    {
        trafficLight!.ChangeTrafficLightState();
        //Console.WriteLine($"Traffic light changed Color from {trafficLight.PreviousState} to {trafficLight.CurrentState}, Event was created at {EndOfAction} at edge {trafficLight.EdgeId}");
        //Console.WriteLine();
        //foreach (var tr in SimulationModel.TrafficLights)
        //{
        //    Console.WriteLine($"Traffic light: {tr.EdgeId,-30}, Color: {tr.CurrentState} at {EndOfAction}");
        //}
        var endOfNext = endTime + trafficLight.States[trafficLight.CurrentState];
        if (endOfNext >= simulationTime)
            return;
        _trafficLightQueue[endOfNext].Add(new Event()
        {
            Action = () => ChangeTrafficLightState(trafficLight, endOfNext),
            Duration = trafficLight.States[trafficLight.CurrentState],
            EndTime = endOfNext,
            StartTime = endTime,
            Type = EventsType.TrafficLightSwitch
        });
    }
    #endregion


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
                Type = EventsType.TrafficLightSwitch
            });
        }


        var pedestrianFlowsList = SimulationModel.Points
            .Where(p => p.Value.PedestriansFlow is not null)
            .GroupBy(p => p.Value.PedestriansFlow)
            .Select(group => group.Select(p => p.Value).ToList())
            .ToList();

        foreach (var points in pedestrianFlowsList)
        {

            _pedestrianQueue[1].Add(new Event()
            {
                Action = () => GeneratePedestrian(points, endTime: 1),
                Duration = 1,
                StartTime = 0,
                EndTime = 1,
                Type = EventsType.PedestrianGenerate
            });
        }

        foreach (var flow in SimulationModel.Flows)
        {
            _vehicleQueue[1].Add(new Event()
            {
                Action = () =>GenerateVehicles(flow,1),
                Duration = 1,
                StartTime = 0,
                EndTime = 1,
                Type = EventsType.VehicleGenerate
            });
        }


    }
    #region Pedestrians
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
            Type = EventsType.PedestrianGenerate
        });

        var flow = crosswalk
            .FirstOrDefault()?.PedestriansFlow?
            .Where(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second <= endTime)
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
            point.NumberOfPedestrians += flow.Value / 60.0;
        }

        StartCrossTheRoad(crosswalk, trafficLights, endTime);
        
    }

    public void StartCrossTheRoad(List<Point> crosswalk, List<TrafficLight> trafficLights, int EndTime)
    {
        var isPossibleToCross = trafficLights.All(tl => tl.CurrentState == TrafficLightState.Red);

        if (isPossibleToCross)
        {
            var tempPedestrians = (int)crosswalk.FirstOrDefault()!.NumberOfPedestrians;
            foreach (var point in crosswalk)
            {
                point.PedestriansOnTheRoad += (int)point.NumberOfPedestrians;
                point.NumberOfPedestrians = point.NumberOfPedestrians - (int)point.NumberOfPedestrians;
            }

            for (int i = 0; i < tempPedestrians; i++)
            {
                var crossingTime = _random.Next(5, 10) * crosswalk.Count;
                var endOfNext = EndTime + crossingTime;
                if (endOfNext >= simulationTime)
                    return;
                _pedestrianQueue[endOfNext].Add(new Event()
                {
                    Action = () => CrossTheRoad(crosswalk),
                    Duration = crossingTime,
                    EndTime = endOfNext,
                    StartTime = EndTime,
                    Type = EventsType.PedestrianCross
                });
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
                Type = EventsType.PedestrianTryCross
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
    #endregion

    #region Vehicles
    public void GenerateVehicles(Flow flow, int endTime)
    {
        var nextTime = endTime + 1;
        if (nextTime >= simulationTime)
            return;
        _vehicleQueue[nextTime].Add(new Event()
        {
            Action = () => GenerateVehicles(flow, nextTime),
            Duration = 1,
            EndTime = nextTime,
            StartTime = endTime,
            Type = EventsType.VehicleGenerate
        });
        var generatedNumber = flow
           .Density
           .Where(p => p.Key.Hour * 60 * 60 + p.Key.Minute * 60 + p.Key.Second <= endTime)
           .Select(p => p.Value)
           .LastOrDefault();

        if (generatedNumber == 0)
        {
            return;
        }
        var destEdge = SimulationModel
            .Edges
            .FirstOrDefault(e => e.StartPointId == flow.PointId);
        flow.VehiclesInQueue += generatedNumber / 60.0;
        var carSize = double.Round(_random.NextDouble(), 1) + 4.0;
        if (flow.VehiclesInQueue >= 1.0)
        {

            var possibleRoutes = SimulationModel
                    .Routes
                    .Where(r => r[0].Id == flow.PointId)
                    .ToList();
            var generated = 0;
            for (int i = 0; i < (int)flow.VehiclesInQueue; i++)
            {
                if (destEdge!.CheckIfEdgeIsFree(carSize))
                {
                    var randomRouteIndex = _random.Next(0, possibleRoutes.Count + 1);
                    var route = SimulationModel.Routes[randomRouteIndex];
                    var vehicle = destEdge.AddVehicle(carSize, route);
                    nextTime = endTime + 1;
                    if (nextTime >= simulationTime)
                        return;
                    _vehicleQueue[nextTime].Add(new Event()
                    {
                        Action = () => MoveVehicle(vehicle, destEdge, nextTime),
                        Duration = 1,
                        EndTime = nextTime,
                        StartTime = endTime,
                        Type = EventsType.VehicleMove
                    });
                    generated++;
                }
                else
                {

                    nextTime = endTime + 1;
                    if (nextTime >= simulationTime)
                        return;
                    _vehicleQueue[nextTime].Add(new Event()
                    {
                        Action = () => VehicleWaitGenerate(flow, destEdge, nextTime),
                        Duration = 1,
                        EndTime = nextTime,
                        StartTime = endTime,
                        Type = EventsType.VehicleWaitGenerate
                    });
                }
            }

            flow.VehiclesInQueue -= generated;



        }

    }

    public void VehicleToNext(Vehicle vehicle, Edge lastEdge, Edge nextEdge, int endTime)
    {
        var nextTime = endTime + 1;
        if (nextTime >= simulationTime)
            return;
        if (nextEdge.CheckIfEdgeIsFree(vehicle.CarSize))
        {
            lastEdge.RemoveVehicle();
            nextEdge.AddVehicle(vehicle);
            _vehicleQueue[nextTime].Add(new Event()
            {
                Action = () => MoveVehicle(vehicle, nextEdge, nextTime),
                Duration = 1,
                EndTime = nextTime,
                StartTime = endTime,
                Type = EventsType.VehicleMove
            });

        }
        else
        {
            _vehicleQueue[nextTime].Add(new Event()
            {
                Action = () => VehicleToNext(vehicle,lastEdge, nextEdge, nextTime),
                Duration = 1,
                EndTime = nextTime,
                StartTime = endTime,
                Type = EventsType.VehicleToNext
            });
        }
    }

    public void MoveVehicle(Vehicle vehicle, Edge edge, int endTime)
    {
        if (vehicle.FrontCar is not null)
        {

            if (vehicle.TillEnd - vehicle.FrontCar?.TillEnd >= edge.Speed)
            {
                vehicle.TillEnd -= edge.Speed;
            }

            var nextTime = endTime + 1;
            if (nextTime >= simulationTime)
                return;
            _vehicleQueue[nextTime].Add(new Event()
            {
                Action = () => MoveVehicle(vehicle, edge, nextTime),
                Duration = 1,
                EndTime = nextTime,
                StartTime = endTime,
                Type = EventsType.VehicleMove
            });

        }
        else
        {
            var nextTime = endTime + 1;
            if (nextTime >= simulationTime)
                return;
            if (vehicle.TillEnd > 0.0)
            {
                vehicle.TillEnd -= edge.Speed;
                _vehicleQueue[nextTime].Add(new Event()
                {
                    Action = () => MoveVehicle(vehicle, edge, nextTime),
                    Duration = 1,
                    EndTime = nextTime,
                    StartTime = endTime,
                    Type = EventsType.VehicleMove
                });
            }
            else
            {
                var nextPoint = vehicle.GetNextPoint();
                var nextEdge = vehicle.GetNextEdge();
                if (nextPoint is null || nextEdge is null)
                {
                    vehicle = null;
                    edge.RemoveVehicle();
                    return;
                }
                if (nextPoint.IsPossibleToDrive() && nextEdge.IsPossibleToDrive())
                {
                    _vehicleQueue[nextTime].Add(new Event()
                    {
                        Action = () => VehicleToNext(vehicle, edge, nextEdge, nextTime),
                        Duration = 1,
                        EndTime = nextTime,
                        StartTime = endTime,
                        Type = EventsType.VehicleToNext
                    });

                }
                else
                {
                    _vehicleQueue[nextTime].Add(new Event()
                    {
                        Action = () => MoveVehicle(vehicle, edge, nextTime),
                        Duration = 1,
                        EndTime = nextTime,
                        StartTime = endTime,
                        Type = EventsType.VehicleWaitNext
                    });
                }

            }
        }

    }

    public void VehicleWaitGenerate(Flow flow, Edge destEdge, int endTime)
    {
        var carSize = double.Round(_random.NextDouble(), 1) + 4.0;
        if (destEdge!.CheckIfEdgeIsFree(carSize))
        {
            var possibleRoutes = SimulationModel
                  .Routes
                  .Where(r => r[0].Id == flow.PointId);
            var randomRouteIndex = _random.Next(0, possibleRoutes.Count());
            var route = SimulationModel.Routes[randomRouteIndex];
            var vehicle = destEdge.AddVehicle(carSize, route);
            var nextTime = endTime + 1;
            if (nextTime >= simulationTime)
                return;
            _vehicleQueue[nextTime].Add(new Event()
            {
                Action = () => MoveVehicle(vehicle, destEdge, nextTime),
                Duration = 1,
                EndTime = nextTime,
                StartTime = endTime,
                Type = EventsType.VehicleMove
            });
        }
        else
        {

            var nextTime = endTime + 1;
            if (nextTime >= simulationTime)
                return;
            _vehicleQueue[nextTime].Add(new Event()
            {
                Action = () => VehicleWaitGenerate(flow, destEdge, nextTime),
                Duration = 1,
                EndTime = nextTime,
                StartTime = endTime,
                Type = EventsType.VehicleWaitGenerate
            });
        }
    }
    #endregion

}
