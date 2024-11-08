using Simulator.Model.Dtos.Request;

namespace Simulator.Utils.SimulationParamsGenerators.Interface;

public interface ISimulationParamsGenerator
{
    string GetSimulationParamsJson();
    SimulationParamsRequestTo GetSimulationParams();
}
