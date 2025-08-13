using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers;

[Route("/vendors")]
public class VendorsController(VendorService vendorService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync() =>
        Json(await vendorService.GetAllAsync());
}