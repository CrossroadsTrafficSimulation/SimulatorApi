namespace Simulator.Model.Entites;

public class Edge()
{
    public string Id { get; set; } = null!;
    public Point StartPoint { get; set; } = null!;
    public string StartPointId { get; set; } = null!;
    public Point EndPoint { get; set; } = null!;
    public string EndPointId { get; set; } = null!;
    public double SpeedLimit { get; set; }
    public double Distance { get; set; }
    public TrafficLight? TrafficLight { get; set; } = null;
    public Queue<Vehicle> Vehicles { get; set; } = [];
    public double SumCarsLength { get; set; } = 0.0;

    public Edge(string id, Point startPoint, Point endPoint, double speed, double distance, TrafficLight? trafficLight = null) : this()
    {
        Id = id;
        StartPoint = startPoint;
        StartPointId = startPoint.Id;
        EndPoint = endPoint;
        EndPointId = endPoint.Id;
        SpeedLimit = speed;
        Distance = distance;
        TrafficLight = trafficLight;
    }

    public bool IsEdgeFree(double carSize)
    {
        return SumCarsLength + carSize < Distance || Vehicles.Count == 0;
    }

    public bool IsAllowedToDriveThrough(double carSize)
    {
        if (TrafficLight is null)
        {
            return IsEdgeFree(carSize);
        }
        return IsEdgeFree(carSize) && TrafficLight.CurrentState == Enums.TrafficLightState.Green;
    }

    public bool TryEnqueueVehicle(Vehicle vehicle)
    {
        if (IsEdgeFree(vehicle.Size))
        {
            Vehicles.Enqueue(vehicle);
            SumCarsLength += vehicle.Size;

            return true;
        }

        return false;
    }

    public bool TryDequeueVehicle(out Vehicle? vehicle)
    {
        var res = Vehicles.TryDequeue(out vehicle);
        if (res)
        {
            SumCarsLength -= vehicle!.Size;

            if (SumCarsLength < 0)
            {
                SumCarsLength = 0;
            }
        }

        return res;
    }
}
