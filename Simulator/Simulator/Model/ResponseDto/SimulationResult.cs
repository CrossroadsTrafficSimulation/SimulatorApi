namespace Simulator.Model.ResponseDto;

public record SimulationResult(string PointId, 
    List<(int hour, double averageSize)> QueueAverageSizeByHours, 
    List<(int hour, double averageTrafficLightTime)> TrafficLightAverageTimeByHours)
{
}
