using AutoMapper;
using Simulator.Model.Dtos.Response;
using Simulator.Processes;
using Simulator.Services.Interface;

namespace Simulator.Services.Implementation;

public class SimulationService(IMapper mapper) : ISimulationService
{
    private readonly List<Simulation> _simulations = null!;
    private const int SimulationTime = 24 * 60 * 60;
    //public void SetUpSimulations(List<> flows,List<DTO.Edge> edges, List<DTO.Point> points, List<DTO.TrafficLight> trafficLights,int simulationsQuantity = 5)
    //{
    //    var rand = new Random();
    //    _simulations = new List<Simulation>(simulationsQuantity);
    //    for (int i = 0; i < simulationsQuantity; i++)
    //    {
    //        _simulations.Add(new Simulation()
    //        {
    //            Edges = edges.Select(e => mapper.Map<PROC.Edge>(e)).ToList(),
    //            Points = edges.Select(p => mapper.Map<PROC.Point>(p)).ToList(),
    //            TrafficLights = edges.Select(tr => mapper.Map<PROC.TrafficLight>(tr)).ToList(),
    //            Flows = edges.Select(f => mapper.Map<PROC.Flow>(f)).ToList()
    //        });
    //    }
    //}
    private SimulationResponseTo RunSimulation(Simulation simulation)
    {
        for (int currentTime = 0; currentTime < SimulationTime; currentTime++)
        {
            simulation.ProcessTrafficLights(currentTime);
            simulation.PrecessPedestrians(currentTime);
            simulation.ProcessVehicles(currentTime);
        }
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
