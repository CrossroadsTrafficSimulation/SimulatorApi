using Simulator.Model.Dtos.Request;
using Simulator.Model.Dtos.Response;
using Simulator.Processes;
using Simulator.Services.Interface;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Simulator.Services.Implementation;

public class SimulationService(IModelPreparationService preparationService) : ISimulationService
{
    private readonly List<Simulation> _simulations = [];
    private const int SimulationTime = 24 * 60 * 60;

    public void SetUpSimulations(SimulationParamsRequestTo simulationParams, int simulationQuantity)
    {
        for (int i = 0; i < simulationQuantity; i++)
        {
            _simulations.Add(new Simulation(SimulationTime)
            {
                SimulationModel = preparationService.GetSimulationModel(simulationParams)
            });
        }
    }

    private static SimulationResponseTo RunSimulation(Simulation simulation)
    {
        var watch = new Stopwatch();
        watch.Start();
        simulation.SetUpFirstEvents();
        for (int currentTime = 0; currentTime < SimulationTime; currentTime++)
        {
            simulation.ProcessTrafficLights(currentTime);
            simulation.ProcessPedestrians(currentTime);
            simulation.ProcessVehicles(currentTime);
            simulation.ProcessStatistics(currentTime);
        }
        watch.Stop();
        Console.WriteLine($"Done: {watch.ElapsedMilliseconds}");

        return simulation.GetResult();
    }

    public List<SimulationResponseTo> SimulateTraffic()
    {
        var results = new ConcurrentBag<SimulationResponseTo>();

        _ = Parallel.ForEach(_simulations, (simulation) =>
        {
            results.Add(RunSimulation(simulation));
        });

        return [.. results];
    }
}
