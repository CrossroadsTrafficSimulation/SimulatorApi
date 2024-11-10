using AutoMapper;
using Simulator.Model.Dtos.Request;
using Simulator.Model.Dtos.Response;
using Simulator.Processes;
using Simulator.Services.Interface;

namespace Simulator.Services.Implementation;

public class SimulationService(IMapper mapper, IModelPreparationService preparationService) : ISimulationService
{
    private readonly List<Simulation> _simulations = new List<Simulation>();
    private const int SimulationTime = 24 * 60 * 60;
    public void SetUpSimulations(SimulationParamsRequestTo simulationParams, int simulationQuantity = 5)
    {
        var rand = new Random();
        for (int i = 0; i < simulationQuantity; i++)
        {
            _simulations.Add(new Simulation()
            {
                SimulationModel = preparationService.GetSimulationModel(simulationParams)
            });
        }
    }
    private SimulationResponseTo RunSimulation(Simulation simulation)
    {
        simulation.SetUpFirstEvents();
        for (int currentTime = 0; currentTime < SimulationTime; currentTime++)
        {
            simulation.ProcessTrafficLights(currentTime);
            simulation.ProcessPedestrians(currentTime);
            //simulation.ProcessVehicles(currentTime);
        }
        Console.WriteLine("Done");
        return null!;
    }
    public List<SimulationResponseTo> SimulateTraffic()
    {
        var results = new List<SimulationResponseTo>();

        foreach (var simulation in _simulations)
        {
            results.Add(RunSimulation(simulation));
        }

        return results;
    }

}
