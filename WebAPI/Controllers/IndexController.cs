using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("/")]
public class IndexController : Controller
{
    public async Task<IActionResult> IndexAsync() => new ContentResult();
}