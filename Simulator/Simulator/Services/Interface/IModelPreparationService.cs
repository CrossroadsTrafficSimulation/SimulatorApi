using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Request;

namespace Simulator.Services.Interface;

public interface IModelPreparationService
{
    public SimulationModel GetSimulationModel(SimulationParamsRequestTo simulationParams);
}
