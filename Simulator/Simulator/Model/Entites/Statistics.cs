using System.Text.Json.Serialization;

namespace Simulator.Model.Entites;

public class Statistics
{
    public List<EdgeStatistics> EdgesVehicleDencity { get; set; } = [];
    public List<CrossWalkStatistics> CrossWalkPedestrianDencity { get; set; } = [];
    [JsonIgnore]
    public readonly int StatisticsSampleRate;

    public Statistics(int statisticsSampleRate)
    {
        StatisticsSampleRate = statisticsSampleRate;
    }

    public void CreateAllEdgeStatistics(List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            EdgesVehicleDencity.Add(new EdgeStatistics(StatisticsSampleRate, edge));
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
