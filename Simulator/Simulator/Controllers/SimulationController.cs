using Route = System.Collections.Generic.List<string>;

using Microsoft.AspNetCore.Mvc;
using Simulator.Model.RequestDto;
using Simulator.Services.Interface;

namespace Simulator.Controllers;

[ApiController]
[Route("api/v1.0.0/simulation")]
public class SimulationController(ILogger<SimulationController> logger, ISimulationService service) : ControllerBase
{
    //[HttpGet]
    //[Route("edge")]
    //public IActionResult GetEdge()
    //{
    //    return Ok(new Edge(10.5, 100.0, "A", "B"));   
    //}

    //[HttpGet]
    //[Route("flow")]
    //public IActionResult GetFlow()
    //{
    //    return Ok(new Flow("A", new Dictionary<TimeOnly, double> { { new TimeOnly(10, 20, 30), 10.0 }, { new TimeOnly(11, 21, 31), 20.0 } }));
    //}

    //[HttpGet]
    //[Route("point/traffic-light")]
    //public IActionResult GetPointWithTrafficLight()
    //{
    //    return Ok(new Point("A", new TrafficLight(10, 2, 30), null));
    //}

    //[HttpGet]
    //[Route("point/pedestrian-flow")]
    //public IActionResult GetPointWithPedestrianFlow()
    //{
    //    return Ok(new Point("A", null,
    //        new Dictionary<TimeOnly, double>
    //        {
    //            { new TimeOnly(10, 20, 30), 10.0 },
    //            { new TimeOnly(11, 21, 31), 20.0 }
    //        }));
    //}

    [HttpPost]
    public IActionResult SimulateTraffic()
    {
        var result = service.SimulateTraffic();
        return result is not null ? Ok(result) : BadRequest();
    }
}
