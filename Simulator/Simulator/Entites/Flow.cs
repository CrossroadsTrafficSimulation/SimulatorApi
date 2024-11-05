using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Entites
{
    public class Flow
    {
        public string FlowId { get; set; }
        public string PointId { get; set; }
        public Point Point { get; set; }
        public TimeDensity Density { get; set; }
    }
}
