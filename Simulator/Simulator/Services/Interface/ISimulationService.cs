using Simulator.Model.Dtos.Request;
using Simulator.Model.Dtos.Response;

namespace Simulator.Services.Interface;

public interface ISimulationService
{
    void SetUpSimulations(SimulationParamsRequestTo simulationParams, int simulationQuantity = 1);
    List<SimulationResponseTo> SimulateTraffic();
}
