using System.Text.Json.Serialization;

namespace Simulator.Model.Entites;

public class Statistics
{
    public List<EdgeStatistics> EdgesVehicleDencity { get; set; } = [];
    public List<CrossWalkStatistics> CrossWalkPedestrianDencity { get; set; } = [];
    [JsonIgnore]
    public readonly int StatisticsSampleRate;
    [JsonIgnore]
    public List<Tuple<Edge, EdgeStatistics>> FastEdgeStatistics { get; private set; } = [];

    public Statistics(int statisticsSampleRate)
    {
        StatisticsSampleRate = statisticsSampleRate;
    }

    public void CreateAllEdgeStatistics(List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            var stats = new EdgeStatistics(StatisticsSampleRate, edge);
            EdgesVehicleDencity.Add(stats);
            FastEdgeStatistics.Add(new(edge, stats));
        }
    }

    public void CreateAllCrosswalkStatistics(List<List<Point>> crosswalks)
    {
        foreach (var crosswalk in crosswalks)
        {
            CrossWalkPedestrianDencity.Add(new CrossWalkStatistics(StatisticsSampleRate, crosswalk));
        }
    }
}
