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
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secretKey;

    public TokenService()
    {
        _issuer = Environment.GetEnvironmentVariable("Issuer") ?? throw new NullReferenceException("No issuer provided.");
        _audience = Environment.GetEnvironmentVariable("Audience") ?? throw new NullReferenceException("No audience provided.");
        _secretKey = Environment.GetEnvironmentVariable("SecretKey") ?? throw new NullReferenceException("No secret key provided.");
    }

    public async Task<TokenResponse> GenerateTokenAsync(TokenRequest request, int expiresInDays = 30)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId))
                throw new NullReferenceException("No user id provided.");

            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)), SecurityAlgorithms.HmacSha256) ?? throw new NullReferenceException("Unable to create credentials.");

            using var http = new HttpClient();
            var response = await http.PostAsJsonAsync(Environment.GetEnvironmentVariable("GenerateTokenUri"), request); // TODO: Kolla upp korrekt URL
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
                Issuer = _issuer,
                Audience = _audience,
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

    public async Task<ValidationResponse> ValidateTokenAsync(ValidationRequest request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ClockSkew = TimeSpan.Zero

            }, out SecurityToken validatedToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NullReferenceException("No user id provided.");
            if (userId != request.UserId)
                throw new Exception("User id does not match.");

            using var http = new HttpClient();
            var response = await http.GetAsync(Environment.GetEnvironmentVariable("ValidateTokenUri")); // TODO: Kolla upp korrekt URL
            if (!response.IsSuccessStatusCode)
                throw new NullReferenceException("User not found.");

            return new ValidationResponse { Succeeded = true };
        }
        catch (Exception ex)
        {
            return new ValidationResponse { Succeeded = false, Message = ex.Message };
        }
    }
}
