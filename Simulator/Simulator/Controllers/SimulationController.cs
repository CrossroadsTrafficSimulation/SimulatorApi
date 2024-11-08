using Microsoft.AspNetCore.Mvc;
using Simulator.Model.Dtos.Request;
using Simulator.Services.Interface;
using Simulator.SimulationParamsGenerators;
using Simulator.SimulationParamsGenerators.Implementation;

namespace Simulator.Controllers;

[ApiController]
[Route("api/v1.0.0/simulation")]
public class SimulationController(ILogger<SimulationController> logger, ISimulationService simulationService) : ControllerBase
{
    [HttpPost]
    [Route("run")]
    public IActionResult RunSimulation(
        //[FromBody] SimulationParamsRequestTo simulationParams
        )
    {
        Console.WriteLine(new SimulationParamsGeneratorOne().GetSimulationParamsJson());
        var simulationParams = new SimulationParamsGeneratorOne().GetSimulationParams();
        simulationService.SetUpSimulations(simulationParams);
        var simulationsResults = simulationService.SimulateTraffic();
        return Ok();
    }
}
