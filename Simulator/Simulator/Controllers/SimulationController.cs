using Microsoft.AspNetCore.Mvc;
using Simulator.Model.Dtos.Request;
using Simulator.Services.Interface;

namespace Simulator.Controllers;

[ApiController]
[Route("api/v1.0.0/simulation")]
public class SimulationController(ILogger<SimulationController> logger, ISimulationService simulationService) : ControllerBase
{
    [HttpPost]
    [Route("run")]
    public IActionResult RunSimulation([FromBody] SimulationParamsRequestTo simulationParams)
    {
        return Ok();
    }
}
