namespace PermissionAPI.Models.SpResults;

/// <summary>
/// Keyless entity for dbo.sp_GetUserPermissions result
/// </summary>
public class UserPermissionResult
{
    public string UserName { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
}
