using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Presentation.Functions;
using Presentation.Interfaces;
using Presentation.Models;
using Tests.Helpers;

namespace Tests.Functions;

// Hjälp från ChatGPT med delar av testerna
public class GenerateToken_Tests
{
    private readonly Mock<ILogger<GenerateToken>> _logger = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly GenerateToken _function;

    public GenerateToken_Tests()
    {
        _function = new GenerateToken(_logger.Object, _tokenService.Object);
    }

    [Theory]
    [InlineData("", "Invalid body.")]
    [InlineData("{ not json }", "Invalid JSON format.")]
    public async Task GenerateToken_ShouldReturnBadRequest_WhenInputIsInvalid(string body, string expectedMessage)
    {
        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(body)) as BadRequestObjectResult;
        var response = Assert.IsType<TokenResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.False(response.Succeeded);
        Assert.Equal(expectedMessage, response.Message);
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        _tokenService.Setup(ts => ts.GenerateTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<int>()))
            .ReturnsAsync(new TokenResponse { Succeeded = true, Token = "token123" });
        var request = new TokenRequest { UserId = "user1" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(json)) as OkObjectResult;
        var response = Assert.IsType<TokenResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.True(response.Succeeded);
        Assert.Equal("token123", response.Token);
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        _tokenService.Setup(ts => ts.GenerateTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<int>()))
            .ReturnsAsync(new TokenResponse { Succeeded = false, Message = "Error generating token." });
        var request = new TokenRequest { UserId = "user1" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(json)) as BadRequestObjectResult;
        var response = Assert.IsType<TokenResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.False(response.Succeeded);
        Assert.Equal("Error generating token.", response.Message);
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnInternalServerError_WhenServiceThrowsException()
    {
        // Arrange 
        _tokenService.Setup(ts => ts.GenerateTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Service error."));
        var request = new TokenRequest { UserId = "user1" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(json)) as BadRequestObjectResult;
        var response = Assert.IsType<TokenResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.False(response.Succeeded);
        Assert.Equal("Internal server error.", response.Message);
    }
}
