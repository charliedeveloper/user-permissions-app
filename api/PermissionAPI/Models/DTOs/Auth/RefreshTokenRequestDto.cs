using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs.Auth;

/// <summary>
/// Request to refresh an access token
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Expired or expiring access token
    /// </summary>
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Valid refresh token
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
