using Simulator.Model.Dtos.Request;

namespace TestBench.SimulationParamsGenerators.Interface;

public interface ISimulationParamsGenerator
{
    string GetSimulationParamsJson();
    SimulationParamsRequestTo GetSimulationParams();
}
