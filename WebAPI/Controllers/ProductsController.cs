using Core.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers;

[Route("products")]
public class ProductsController(ProductService productService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync() =>
        Json(await productService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] ProductCreateModel model) =>
        Json(await productService.CreateAsync(model));

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] ProductUpdateModel model) =>
        Json(await productService.UpdateAsync(model));

    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] long productId)
    {
        await productService.DeleteAsync(productId);
        return Ok();
    }

    [HttpGet("{productId:long}")]
    public async Task<IActionResult> GetAsync(long productId) =>
        Json(await productService.GetAsync(productId));

    [HttpGet("{productId}/route")]
    public async Task<IActionResult> GetRouteAsync([FromRoute] long productId) =>
        Json(await productService.GetRouteAsync(productId));
}