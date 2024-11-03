
using Simulator.Model;
using Simulator.Model.ResponseDto;

namespace Simulator.Processes
{
    public class Simulation
    {
        public List<Flow> Flows { get; set; } = null!;
        public List<Edge> Edges { get; set; } = null!;
        public List<Point> Points { get; set; } = null!;
        public List<TrafficLight> TrafficLights { get; set; } = null!;

        private List<Event> _events = [];

        public void ProcessVehicles()
        {
            var vehicleEvents = _events.Where(e => e.Type.Equals("Vehicle", StringComparison.OrdinalIgnoreCase));
        }
        public void ProcessTrafficLights()
        {
            throw new NotImplementedException();
        }

        public void PrecessPedestrians()
        {
            throw new NotImplementedException();
        }

        public SimulationResult GetResult()
        {
            throw new NotImplementedException();
        }
    }
}
