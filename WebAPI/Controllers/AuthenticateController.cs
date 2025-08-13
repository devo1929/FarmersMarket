using Core.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers;

[Route("authenticate")]
public class AuthenticateController(AuthenticateService authenticateService) : Controller
{
    [HttpPost]
    public async Task<IActionResult> IndexAsync([FromBody] AuthenticateRequestModel authenticateRequestModel) =>
        Json(await authenticateService.AuthenticateAsync(authenticateRequestModel));
}