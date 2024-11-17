using Microsoft.AspNetCore.Mvc;
using Simulator.Services.Interface;
using Simulator.Model.Dtos.Request;
using Simulator.Utils.SimulationParamsGenerators.Implementation;
using System.Diagnostics;

namespace Simulator.Controllers;

[ApiController]
[Route("api/v1.0.0/simulation")]
public class SimulationController(ILogger<SimulationController> logger, ISimulationService simulationService) : ControllerBase
{
    [HttpPost]
    [Route("run")]
    public IActionResult RunSimulation(
        [FromBody] SimulationParamsRequestTo simulationParams
        )
    {
        Console.WriteLine(new SimulationParamsGeneratorOne().GetSimulationParamsJson());
        var simulationParamsArtifitial = new SimulationParamsGeneratorOne().GetSimulationParams();
        simulationService.SetUpSimulations(simulationParamsArtifitial);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _ = simulationService.SimulateTraffic();
        
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed);
        
        return Ok();
    }
}
