using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for updating an existing permission
/// </summary>
public class UpdatePermissionDto
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
    /// Indicates if the permission is active
    /// </summary>
    public bool IsActive { get; set; }
}
