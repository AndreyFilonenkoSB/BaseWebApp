using BaseWebApp.Api.Contracts;
using BaseWebApp.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BaseWebApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var result = await _userService.RegisterUserAsync(request.Email, request.Password);
        if (result.IsFailure)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var result = await _userService.LoginAsync(request.Email, request.Password);
        if (result.IsFailure)
        {
            return Unauthorized(result.ErrorMessage);
        }

        var user = result.Value!;
        var token = _tokenService.GenerateToken(user);

        return Ok(new { Token = token });
    }
}