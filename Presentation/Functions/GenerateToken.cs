using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Presentation.Interfaces;
using Presentation.Models;

namespace Presentation.Functions;

public class GenerateToken(ILogger<GenerateToken> logger, ITokenService tokenService)
{
    private readonly ILogger<GenerateToken> _logger = logger;
    private readonly ITokenService _tokenService = tokenService;

    public static BadRequestObjectResult BadRequest(string message) => new(new TokenResponse { Succeeded = false, Message = message });

    [Function("GenerateToken")]
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

            TokenRequest? tokenRequest;
            try
            {
                tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);
                if (tokenRequest == null)
                    throw new JsonException("Deserialization failed.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize request body.");
                return BadRequest("Invalid JSON format.");
            }

            var response = await _tokenService.GenerateTokenAsync(tokenRequest);
            return response.Succeeded
                ? new OkObjectResult(response)
                : BadRequest(response.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in GenerateToken.");
            return BadRequest("Internal server error.");
        }
    }
}