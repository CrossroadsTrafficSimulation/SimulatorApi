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
}
