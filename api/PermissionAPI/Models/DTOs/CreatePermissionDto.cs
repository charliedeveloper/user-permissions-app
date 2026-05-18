using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for creating a new permission
/// </summary>
public class CreatePermissionDto
{
    /// <summary>
    /// Unique permission key (required, max 100 characters)
    /// </summary>
    [Required(ErrorMessage = "Permission key is required")]
    [StringLength(100, ErrorMessage = "Permission key cannot exceed 100 characters")]
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    /// Permission name (required, max 100 characters)
    /// </summary>
    [Required(ErrorMessage = "Permission name is required")]
    [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the permission is active (default: true)
    /// </summary>
    public bool IsActive { get; set; } = true;
}
