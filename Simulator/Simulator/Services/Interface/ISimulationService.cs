using Simulator.Model.ResponseDto;

namespace Simulator.Services.Interface;

public interface ISimulationService
{
    List<SimulationResult> SimulateTraffic();
}
