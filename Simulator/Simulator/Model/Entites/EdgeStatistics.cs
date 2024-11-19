

namespace Simulator.Model.Entites;

public class EdgeStatistics
{
    public string EdgeId { get; set; } = "";

    //Короче, если у нас будет в качестве периода только час, то тогда int самое то, иначе нужно TimeOnly
    public Dictionary<int,double> Statistics { get; set; } = [];
    public TrafficLight? TrafficLight { get; set; } = null;

    public EdgeStatistics(int sampleRate, Edge edge)
    {
        EdgeId = edge.Id;
        var trafficLight = edge.TrafficLight is not null ? new TrafficLight()
        {
            CurrentState = edge.TrafficLight.CurrentState,
            Edge = null,
            EdgeId = null,
            PreviousState = edge.TrafficLight.PreviousState,
            States = edge.TrafficLight.States
        } : null;
        TrafficLight = trafficLight;
        for (int i = 0; i < sampleRate; i++)
        {
            Statistics[i] = 0;
        }
    }
}
