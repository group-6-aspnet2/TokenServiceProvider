using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class TokenResponse
{
    [Required]
    public bool Succeeded { get; set; }

    [Required]
    public string Token { get; set; } = null!;

    public string? Message { get; set; }
}
