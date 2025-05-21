using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Presentation.Functions;
using Presentation.Interfaces;
using Presentation.Models;
using Tests.Helpers;

namespace Tests.Functions;

public class ValidateToken_Tests
{
    private readonly Mock<ILogger<ValidateToken>> _logger = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly ValidateToken _function;

    public ValidateToken_Tests()
    {
        _function = new ValidateToken(_logger.Object, _tokenService.Object);
    }

    [Theory]
    [InlineData("", "Invalid body.")]
    [InlineData("{ not json }", "Invalid JSON format.")]
    public async Task ValidateToken_ShouldReturnBadRequest_WhenInputIsInvalid(string body, string expectedMessage)
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
    public async Task ValidateToken_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        _tokenService.Setup(ts => ts.ValidateTokenAsync(It.IsAny<ValidationRequest>()))
            .ReturnsAsync(new ValidationResponse { Succeeded = true });
        var request = new ValidationRequest { UserId = "user1", Token = "token" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(json)) as OkObjectResult;
        var response = Assert.IsType<ValidationResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.True(response.Succeeded);
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnUnauthorized_WhenServiceFails()
    {
        // Arrange
        _tokenService.Setup(ts => ts.ValidateTokenAsync(It.IsAny<ValidationRequest>()))
            .ReturnsAsync(new ValidationResponse { Succeeded = false, Message = "Invalid or expired token." });
        var request = new ValidationRequest { UserId = "user1", Token = "token" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Act
        var result = await _function.Run(TestHelpers.CreateHttpRequest(json)) as UnauthorizedObjectResult;
        var response = Assert.IsType<ValidationResponse>(result!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
        Assert.False(response.Succeeded);
        Assert.Equal("Invalid or expired token.", response.Message);
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnInternalServerError_WhenServiceThrowsException()
    {
        // Arrange
        _tokenService.Setup(s => s.ValidateTokenAsync(It.IsAny<ValidationRequest>()))
                    .ThrowsAsync(new Exception("Service error."));
        var request = new ValidationRequest { UserId = "user1", Token = "token" };
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
