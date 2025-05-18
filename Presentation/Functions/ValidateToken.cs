using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Presentation.Interfaces;
using Presentation.Models;

namespace Presentation.Functions;

public class ValidateToken(ILogger<ValidateToken> logger, ITokenService tokenService)
{
    private readonly ILogger<ValidateToken> _logger = logger;
    private readonly ITokenService _tokenService = tokenService;

    public static BadRequestObjectResult BadRequest(string message) => new(new TokenResponse { Succeeded = false, Message = message });

    [Function("ValidateToken")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                const string message = "Invalid body.";
                _logger.LogWarning(message);
                return BadRequest(message);
            }

            ValidationRequest? validationRequest;
            try
            {
                validationRequest = JsonConvert.DeserializeObject<ValidationRequest>(body);
                if (validationRequest == null)
                    throw new JsonException("Deserialization failed.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize request body.");
                return BadRequest("Invalid JSON format.");
            }

            var response = await _tokenService.ValidateTokenAsync(validationRequest);
            return response.Succeeded
                ? new OkObjectResult(response)
                : new UnauthorizedObjectResult(new ValidationResponse { Succeeded = false, Message = "Invalid or expired token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in GenerateToken.");
            return BadRequest("Internal server error.");
        }
    }
}