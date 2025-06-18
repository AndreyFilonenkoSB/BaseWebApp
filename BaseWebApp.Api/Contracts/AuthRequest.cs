using System.ComponentModel.DataAnnotations;

namespace BaseWebApp.Api.Contracts;

public record AuthRequest([Required] string Email, [Required] string Password);