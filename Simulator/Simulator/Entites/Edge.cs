

namespace Simulator.Entites
{
    public class Edge
    {
        public double Speed { get; set; } // Это тоже
        public double Distance { get; set; }
        public string StartPointId { get; set; }
        public Point StartPoint { get; set; }
        public string EndPointId { get; set; }
        public Point EndPoint { get; set; }
        public TrafficLight? TrafficLight { get; set; }
        public double CarSize { get; set; } // Лучше потом это в транспорт закинуть, но пока что пофиг
    }
}
