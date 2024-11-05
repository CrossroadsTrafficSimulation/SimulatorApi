using TimeDensity = System.Collections.Generic.Dictionary<System.TimeOnly, double>;

namespace Simulator.Entites
{
    public class Point
    {
        public string Id { get; set; }
        public IList<Edge> Edges { get; set; }
        public IList<Intersection> Intersections { get; set; }
        public string FlowId { get; set; }
        public Flow Flow { get; set; }
        public TimeDensity? PedestriansFlow { get; set; }
    }
}
