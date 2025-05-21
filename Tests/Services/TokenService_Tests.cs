using Presentation.Models;
using Presentation.Services;

namespace Tests.Services;

public class TokenService_Tests
{
    private const string _issuerKey = "Issuer";
    private const string _audienceKey = "Audience";
    private const string _secretKey = "SecretKey";
    private const string _issuerVal = "TestIssuer";
    private const string _audienceVal = "TestAudience";
    private const string _secretVal = "Secret123456";

    [Fact]
    public void Constructor_MissingEnv_ThrowsNullReferenceException()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_issuerKey, null);
        Environment.SetEnvironmentVariable(_audienceKey, null);
        Environment.SetEnvironmentVariable(_secretKey, null);

        // Assert
        Assert.Throws<NullReferenceException>(() => new TokenService());
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnFalse_WhenUserIdIsEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_issuerKey, _issuerVal);
        Environment.SetEnvironmentVariable(_audienceKey, _audienceVal);
        Environment.SetEnvironmentVariable(_secretKey, _secretVal);
        var tokenService = new TokenService();
        
        // Act
        var result = await tokenService.GenerateTokenAsync(new TokenRequest { UserId = string.Empty });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("No user id provided.", result.Message);
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnFalse_WhenTokenIsInvalid()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_issuerKey, _issuerVal);
        Environment.SetEnvironmentVariable(_audienceKey, _audienceVal);
        Environment.SetEnvironmentVariable(_secretKey, _secretVal);
        var tokenService = new TokenService();

        // Act
        var result = await tokenService.ValidateTokenAsync(new ValidationRequest { UserId = "user1", Token = "invalidToken" });

        // Assert
        Assert.False(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.Message));
    }
}
