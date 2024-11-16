namespace Simulator.Model;

using Simulator.Model.Entites;
using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;

public class Vehicle
{
    public Route Route { get; set; } = null!;
    public double CarSize { get; set; } = 0.0;
    public double TillEnd { get; set; } = 0.0;
    public Vehicle? FrontCar { get; set; } = null;
    public int CurrentPoint { get; set; } = 0;
    public string? CurrentEdgeId { get; set; } = null;

    public Point? GetNextPoint()
    {
        CurrentPoint++;
        if (CurrentPoint < Route.Count - 1)
        {
            return Route[CurrentPoint];
        }

        return null;
    }

    public Point? GetCurrentPoint()
    {
        if (CurrentPoint <= Route.Count)
        {
            return Route[CurrentPoint];
        }
        return null;
    }

    public Edge? GetNextEdge()
    {
        if (CurrentPoint < Route.Count - 1)
        {
            var startPoint = Route[CurrentPoint];
            var endPoint = Route[CurrentPoint + 1];
            return startPoint
                        .Edges
                        .FirstOrDefault(e => e.StartPointId == startPoint.Id
                                             && e.EndPointId == endPoint.Id);
        }

        return null;
    }

}
