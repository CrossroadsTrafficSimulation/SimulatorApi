namespace Simulator.Entites
{
    public class TrafficLight
    {
        public string TrafficLightId { get; set; }
        public List<int> States { get; set; }
        public int CurrentState { get; set; } = 0;
        public string EdgeId { get; set; }
        public Edge Edge { get; set; }
    }
}
