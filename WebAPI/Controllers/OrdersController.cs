using Core.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers;

[Route("orders")]
public class OrdersController(OrderService orderService) : Controller
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] OrderCreateModel order) =>
        Json(await orderService.CreateAsync(order));

    [HttpGet]
    public async Task<IActionResult> GetAllAsync() =>
        Json(await orderService.GetAllAsync());

    [HttpGet("{orderId:long}/route")]
    public async Task<IActionResult> GetRouteAsync(long orderId) =>
        Json(await orderService.GetRouteAsync(orderId));
}