using Microsoft.AspNetCore.Mvc;
using Simulator.Model.Dtos.Request;
using Simulator.Services.Interface;
using Simulator.Model.Dtos.Request;
using Simulator.Utils.SimulationParamsGenerators.Implementation;
using System.Diagnostics;

namespace Simulator.Controllers;

[ApiController]
[Route("api/v1.0.0/simulation")]
public class SimulationController(ISimulationService simulationService) : ControllerBase
{
    [HttpPost]
    [Route("run")]
    public IActionResult RunSimulation(
        [FromBody] SimulationParamsRequestTo simulationParams
        )
    {
        Console.WriteLine(new SimulationParamsGeneratorCrossroad().GetSimulationParamsJson());
        var simulationParams = new SimulationParamsGeneratorCrossroad().GetSimulationParams();
        simulationService.SetUpSimulations(simulationParams);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var result = simulationService.SimulateTraffic();

        stopwatch.Stop();
        Console.WriteLine($"TOTAL: {stopwatch.Elapsed}");

        return Ok(result);
    }
}
