using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.Services.Interfaces;

namespace TechsysLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/deliveries")]
public sealed class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveries;

    public DeliveriesController(IDeliveryService deliveries) => _deliveries = deliveries;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeliveryRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub")!;
        await _deliveries.RegisterAsync(request, userId, ct);
        return Ok(new { ok = true });
    }
}
