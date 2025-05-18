using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class ValidationRequest
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;
}
