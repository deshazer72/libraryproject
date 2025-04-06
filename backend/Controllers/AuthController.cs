using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuth _authServices;

    public AuthController(IAuth authServices)
    {
        _authServices = authServices;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var registerUser = await _authServices.RegisterAsync(model);
        return Ok(registerUser);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
       var token = await _authServices.LoginAsync(model.Email, model.Password);

       if (token == null)
            return Unauthorized(new { Status = "Error", Message = "Invalid username or password!" });

        // Return the token and user details
        var jwtToken = token.GetType().GetProperty("token")?.GetValue(token, null);
        var expiration = token.GetType().GetProperty("expiration")?.GetValue(token, null);
        var user = token.GetType().GetProperty("user")?.GetValue(token, null);

        if (jwtToken == null || expiration == null || user == null)
            return BadRequest(new { Status = "Error", Message = "Failed to retrieve token or user details." });

       return Ok(token);
    }
}

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Default role
}

public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}