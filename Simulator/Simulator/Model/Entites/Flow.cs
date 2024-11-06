using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Model.Entites;

public class Flow()
{
    public Point Point { get; set; } = null!;
    public string PointId { get; set; } = null!;
    public TimeDensity Density { get; set; } = null!;

    public Flow(Point point, TimeDensity density) : this()
    {
        Point = point;
        Density = density;
        PointId = point.Id;
    }
}
