using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for assigning a user to a group
/// </summary>
public class AssignUserToGroupDto
{
    /// <summary>
    /// Group identifier
    /// </summary>
    [Required(ErrorMessage = "Group ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Group ID must be a positive integer")]
    public int GroupId { get; set; }
}
