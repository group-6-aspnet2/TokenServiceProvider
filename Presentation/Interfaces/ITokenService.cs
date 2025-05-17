using Presentation.Models;

namespace Presentation.Interfaces;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokenAsync(TokenRequest request, int expiresInDays = 30);
}