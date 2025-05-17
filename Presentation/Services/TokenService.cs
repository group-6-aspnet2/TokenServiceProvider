using Microsoft.IdentityModel.Tokens;
using Presentation.Interfaces;
using Presentation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace Presentation.Services;

public class TokenService : ITokenService
{
    public async Task<TokenResponse> GenerateTokenAsync(TokenRequest request, int expiresInDays = 30)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId))
                throw new NullReferenceException("No user id provided.");

            var issuer = Environment.GetEnvironmentVariable("Issuer") ?? throw new NullReferenceException("No issuer provided.");
            var audience = Environment.GetEnvironmentVariable("Audience") ?? throw new NullReferenceException("No audience provided.");
            var secretKey = Environment.GetEnvironmentVariable("SecretKey") ?? throw new NullReferenceException("No secret key provided.");
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256) ?? throw new NullReferenceException("Unable to create credentials.");

            using var http = new HttpClient();
            var response = await http.PostAsJsonAsync("", request);
            if (!response.IsSuccessStatusCode)
                throw new Exception("UserId is invalid.");

            List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, request.UserId)];

            if (!string.IsNullOrEmpty(request.Email))
                claims.Add(new Claim(ClaimTypes.Email, request.Email));

            if (!string.IsNullOrEmpty(request.Role))
                claims.Add(new Claim(ClaimTypes.Role, request.Role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddDays(expiresInDays)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenResponse { Succeeded = true, Token = tokenHandler.WriteToken(token) };
        }
        catch (Exception ex)
        {
            return new TokenResponse { Succeeded = false, Message = ex.Message };
        }
    }
}
