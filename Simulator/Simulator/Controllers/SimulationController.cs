using Microsoft.AspNetCore.Mvc;
using Simulator.Model.Dtos.Request;
using Simulator.Services.Interface;
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
        //[FromBody] SimulationParamsRequestTo simulationParams
        )
    {
        Console.WriteLine(new SimulationParamsGenertorOneCrossing().GetSimulationParamsJson());
        var simulationParams = new SimulationParamsGenertorOneCrossing().GetSimulationParams();
        simulationService.SetUpSimulations(simulationParams);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var result = simulationService.SimulateTraffic();

        stopwatch.Stop();
        Console.WriteLine($"TOTAL: {stopwatch.Elapsed}");

        return Ok(result);
    }
}
