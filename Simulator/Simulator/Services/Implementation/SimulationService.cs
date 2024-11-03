using DTO = Simulator.Model.RequestDto;
using PROC = Simulator.Processes;

using Simulator.Model.RequestDto;
using Simulator.Model.ResponseDto;
using Simulator.Processes;
using Simulator.Services.Interface;
using Simulator.Utils;
using AutoMapper;

namespace Simulator.Services.Implementation;

public class SimulationService(IMapper mapper) : ISimulationService
{
    private List<Simulation> _simulations = null!;
    private const int SimulationTime = 24 * 60 * 60;
    public void SetUpSimulations(List<DTO.Flow> flows,List<DTO.Edge> edges, List<DTO.Point> points, List<DTO.TrafficLight> trafficLights,int simulationsQuantity = 5)
    {
        var rand = new Random();
        _simulations = new List<Simulation>(simulationsQuantity);
        for (int i = 0; i < simulationsQuantity; i++)
        {
            _simulations.Add(new Simulation()
            {
                Edges = edges.Select(e => mapper.Map<PROC.Edge>(e)).ToList(),
                Points = edges.Select(p => mapper.Map<PROC.Point>(p)).ToList(),
                TrafficLights = edges.Select(tr => mapper.Map<PROC.TrafficLight>(tr)).ToList(),
                Flows = edges.Select(f => mapper.Map<PROC.Flow>(f)).ToList()
            });
        }
    }
    private SimulationResult RunSimulation(Simulation simulation)
    {
        for (int i = 0; i < SimulationTime; i++)
        {
            simulation.ProcessTrafficLights();
            simulation.PrecessPedestrians();
            simulation.ProcessVehicles();
        }
        return null!;
    }
    public List<SimulationResult> SimulateTraffic()
    {
        var results = new List<SimulationResult>();
        foreach (var simulation in _simulations)
        {
            results.Add(RunSimulation(simulation));
        }

        return results;
    }

}
