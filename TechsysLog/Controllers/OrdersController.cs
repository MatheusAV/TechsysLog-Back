using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Application.Services.Interfaces;

namespace TechsysLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { error = "Token sem claim 'sub'." });
        await _orders.CreateAsync(request, userId, ct);
        return Created("", new { ok = true });
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> List(CancellationToken ct)
    {
        var list = await _orders.ListAsync(ct);
        return Ok(list);
    }
}
