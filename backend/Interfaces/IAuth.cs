using System.IdentityModel.Tokens.Jwt;
using LibraryAPI.Controllers;
using Microsoft.IdentityModel.Tokens;

public interface IAuth
{
    Task<object> RegisterAsync(RegisterModel model);
    Task<object> LoginAsync(string email, string password);
    Task LogoutAsync();
}