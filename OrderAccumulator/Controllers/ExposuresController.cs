using Microsoft.AspNetCore.Mvc;
using OrderAccumulator.Services;

namespace OrderAccumulator.Controllers;

[ApiController]
[Route("api/exposures")]
public sealed class ExposuresController(FixOrderAcceptor acceptor) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(acceptor.GetExposures());
}
