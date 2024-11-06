using Simulator.Model.Dtos.Response;

namespace Simulator.Services.Interface;

public interface ISimulationService
{
    List<SimulationResponseTo> SimulateTraffic();
}
