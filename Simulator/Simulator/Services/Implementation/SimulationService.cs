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
        var trafficLightsWatch = new Stopwatch();
        var pedestriansWatch = new Stopwatch();
        var vehiclesWatch = new Stopwatch();
        var staticticsWatch = new Stopwatch();
        var watch = new Stopwatch();

        watch.Start();
        simulation.SetUpFirstEvents();

        for (int currentTime = 0; currentTime < SimulationTime; currentTime++)
        {
            trafficLightsWatch.Start();
            simulation.ProcessTrafficLights(currentTime);
            trafficLightsWatch.Stop();

            pedestriansWatch.Start();
            simulation.ProcessPedestrians(currentTime);
            pedestriansWatch.Stop();

            vehiclesWatch.Start();
            simulation.ProcessVehicles(currentTime);
            vehiclesWatch.Stop();

            staticticsWatch.Start();
            simulation.ProcessStatistics(currentTime);
            staticticsWatch.Stop();
        }
        watch.Stop();

        Console.WriteLine($"Traffic lights: {trafficLightsWatch.Elapsed} {100 * (double)trafficLightsWatch.ElapsedMilliseconds / watch.ElapsedMilliseconds:F1}%");
        Console.WriteLine($"Pedestrians: {pedestriansWatch.Elapsed} {100 * (double)pedestriansWatch.ElapsedMilliseconds / watch.ElapsedMilliseconds:F1}%");
        Console.WriteLine($"Vehicles: {vehiclesWatch.Elapsed} {100 * (double)vehiclesWatch.ElapsedMilliseconds / watch.ElapsedMilliseconds:F1}%");
        Console.WriteLine($"Statistics: {staticticsWatch.Elapsed} {100 * (double)staticticsWatch.ElapsedMilliseconds / watch.ElapsedMilliseconds:F1}%");
        Console.WriteLine($"Done: {watch.ElapsedMilliseconds}");

        return simulation.GetResult();
    }

    public List<SimulationResponseTo> SimulateTraffic()
    {
        var results = new ConcurrentBag<SimulationResponseTo>();

        /*        foreach (var simulation in _simulations)
                {
                    results.Add(RunSimulation(simulation));
                }*/

        _ = Parallel.ForEach(_simulations,
            (simulation) =>
            {
                results.Add(RunSimulation(simulation));
            });

        return [.. results];
    }
}
