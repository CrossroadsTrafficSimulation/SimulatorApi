using Simulator.Model.Entites;
using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;

namespace Simulator.Model;
public class Vehicle
{
    public Route Route { get; set; } = null!;
    public double Size { get; set; } = 0.0;
    public int CurrentRoutePos { get; set; } = 0;
    public string? CurrentEdgeId { get; set; } = null;

    public Point? CurrentPoint => CurrentRoutePos <= Route.Count ? Route[CurrentRoutePos] : null;

    public Edge? CurrentEdge
    {
        get
        {
            if (CurrentRoutePos >= Route.Count - 1)
            {
                return null;
            }

            var startPoint = Route[CurrentRoutePos];
            var endPoint = Route[CurrentRoutePos + 1];

            return startPoint.Edges.FirstOrDefault(e => e.StartPointId == startPoint.Id && e.EndPointId == endPoint.Id);
        }
    }

    public Edge? NextEdge
    {
        get
        {
            if (CurrentRoutePos + 1 >= Route.Count - 1)
            {
                return null;
            }

            var startPoint = Route[CurrentRoutePos + 1];
            var endPoint = Route[CurrentRoutePos + 2];

            return startPoint.Edges.FirstOrDefault(e => e.StartPointId == startPoint.Id && e.EndPointId == endPoint.Id);
        }
    }

    public bool IsLastPoint => CurrentRoutePos >= Route.Count - 1;

    public bool TryDriveThrough()
    {
        Console.WriteLine($"Driving through: {CurrentRoutePos}, to {CurrentRoutePos + 1} size {Route.Count} no pedestrians {CurrentPoint!.IsPossibleToDriveThrough()}, allowed {CurrentEdge!.IsAllowedToDriveThrough(Size)}");

        // Route.Count == 4 => 3 edges
        if (CurrentRoutePos + 1 >= Route.Count - 1)
        {
            return false;
        }

        var res = CurrentPoint!.IsPossibleToDriveThrough() && CurrentEdge!.IsAllowedToDriveThrough(Size);
/*        if (res)
        {
            CurrentRoutePos++;
        } */

        return res;
    }
}
