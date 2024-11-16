using Nito.Collections;
using Simulator.Model.Enums;
using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;

namespace Simulator.Model.Entites;

public class Edge()
{
    public string Id { get; set; } = null!;
    public Point StartPoint { get; set; } = null!;
    public string StartPointId { get; set; } = null!;
    public Point EndPoint { get; set; } = null!;
    public string EndPointId { get; set; } = null!;
    public double Speed { get; set; }
    public double Distance { get; set; }
    public TrafficLight? TrafficLight { get; set; } = null;
    public Deque<Vehicle> Vehicles { get; set; } = [];
    public double SumCarsLength { get; set; } = 0.0;

    public Edge(string id, Point startPoint, Point endPoint, double speed, double distance, TrafficLight? trafficLight = null) : this()
    {
        Id = id;
        StartPoint = startPoint;
        StartPointId = startPoint.Id;
        EndPoint = endPoint;
        EndPointId = endPoint.Id;
        Speed = speed;
        Distance = distance;
        TrafficLight = trafficLight;
    }

    public bool CheckIfEdgeIsFree(double carSize)
    {
        if (SumCarsLength >= Distance || Distance - SumCarsLength <= carSize * 0.9)
        {
            return false;
        }
        
        return true;
    }

    public Vehicle AddVehicle(double carSize, Route route)
    {
        var vehicle = new Vehicle()
        {
            CarSize = carSize,
            Route = route,
            TillEnd = Distance
        };
        if (Vehicles.Count > 0)
        {
            vehicle.FrontCar = Vehicles.Last();
        }
       
        Vehicles.AddToBack(vehicle);
        SumCarsLength += carSize;
        return vehicle;
    }

    public Vehicle AddVehicle(Vehicle vehicle)
    {
        vehicle.TillEnd = Distance;
        if (Vehicles.Count > 0)
        {
            vehicle.FrontCar = Vehicles.Last();
        }

        Vehicles.AddToBack(vehicle);
        SumCarsLength += vehicle.CarSize;
        return vehicle;
    }

    public Vehicle RemoveVehicle()
    {
        var vehicle = Vehicles.RemoveFromFront();
        if (Vehicles.Count != 0)
        {
            Vehicles[0].FrontCar = null;
        }
        SumCarsLength -= vehicle.CarSize;
        if (SumCarsLength < 0)
        {
            SumCarsLength = 0;
        }

        return vehicle;
    }

    public bool CheckIfAchiveEnd()
    {
        if (Vehicles.Count == 0)
        {
            return false;
        }
        var vehicle = Vehicles[0];
        if (vehicle.TillEnd <= 0.0)
        {
            return true;
        }

        return false;
    }

    public bool IsPossibleToDrive()
    {
        if (TrafficLight is null)
        {
            return true;
        }
        if (TrafficLight?.CurrentState == TrafficLightState.Green)
        {
            return true;
        }
        return false;
    }
}
