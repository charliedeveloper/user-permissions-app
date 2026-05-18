namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for assigning groups and direct permissions to a user in a single request
/// </summary>
public class AssignUserRolesDto
{
    /// <summary>
    /// List of group IDs to assign to the user
    /// </summary>
    public List<int> GroupIds { get; set; } = new();

    /// <summary>
    /// List of permission IDs to assign directly to the user
    /// </summary>
    public List<int> DirectPermissionIds { get; set; } = new();
}
