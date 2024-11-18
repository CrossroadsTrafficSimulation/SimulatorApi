namespace Simulator.Model.Entites;

public class CrossWalkStatistics
{
    public List<string> Croswalk { get; set; } = [];
    //Короче, если у нас будет в качестве периода только час, то тогда int самое то, иначе нужно TimeOnly
    public Dictionary<int, double> Statistics { get; set; } = [];
}
