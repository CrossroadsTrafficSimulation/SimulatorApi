namespace Simulator.Model.Entites;

internal class Statistics
{
    public List<EdgeStatistics> EdgesVehicleDencity { get; set; } = [];
    public List<CrossWalkStatistics> CrossWalkPedestrianDencity { get; set; } = [];
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
}
