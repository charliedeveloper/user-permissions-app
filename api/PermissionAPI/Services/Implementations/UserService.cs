using Microsoft.EntityFrameworkCore;
using PermissionAPI.Data;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Services.Implementations;

/// <summary>
/// Implementation of user service
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all users");

        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DateHired = u.DateHired
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} users", users.Count);
        return users.AsReadOnly();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user with ID {UserId}", userId);

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DateHired = u.DateHired
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
        }

        return user;
    }

    public async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == userId, cancellationToken);
    }

    // User-Permission management (UserDirectPermissions table)
    public async Task<bool> AssignDirectPermissionToUserAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning direct permission {PermissionId} to user {UserId}", permissionId, userId);

        // Check if the relationship already exists
        var exists = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Permissions)
            .AnyAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Permission {PermissionId} is already assigned to user {UserId}", permissionId, userId);
            return false;
        }

        // Load the user with permissions
        var user = await _context.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found", permissionId);
            return false;
        }

        user.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} assigned to user {UserId} successfully", permissionId, userId);
        return true;
    }

    public async Task<bool> RemoveDirectPermissionFromUserAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing direct permission {PermissionId} from user {UserId}", permissionId, userId);

        var user = await _context.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        var permission = user.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not assigned to user {UserId}", permissionId, userId);
            return false;
        }

        user.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} removed from user {UserId} successfully", permissionId, userId);
        return true;
    }

    public async Task<IReadOnlyList<PermissionDetailDto>> GetUserDirectPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching direct permissions for user {UserId}", userId);

        var permissions = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Permissions)
            .OrderBy(p => p.PermissionKey)
            .Select(p => new PermissionDetailDto
            {
                PermissionId = p.PermissionId,
                PermissionKey = p.PermissionKey,
                PermissionName = p.PermissionName,
                IsActive = p.IsActive ?? true
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} direct permissions for user {UserId}", permissions.Count, userId);
        return permissions.AsReadOnly();
    }

    // User-Group management (UserGroups table)
    public async Task<bool> AssignUserToGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning user {UserId} to group {GroupId}", userId, groupId);

        // Check if the relationship already exists
        var exists = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Groups)
            .AnyAsync(g => g.GroupId == groupId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("User {UserId} is already in group {GroupId}", userId, groupId);
            return false;
        }

        // Load the user with groups
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return false;
        }

        user.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} assigned to group {GroupId} successfully", userId, groupId);
        return true;
    }

    public async Task<bool> RemoveUserFromGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing user {UserId} from group {GroupId}", userId, groupId);

        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        var group = user.Groups.FirstOrDefault(g => g.GroupId == groupId);

        if (group == null)
        {
            _logger.LogWarning("User {UserId} not in group {GroupId}", userId, groupId);
            return false;
        }

        user.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} removed from group {GroupId} successfully", userId, groupId);
        return true;
    }

    public async Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching groups for user {UserId}", userId);

        var groups = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Groups)
            .OrderBy(g => g.GroupName)
            .Select(g => new GroupDto
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                IsActive = g.IsActive ?? true
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} groups for user {UserId}", groups.Count, userId);
        return groups.AsReadOnly();
    }

    // Bulk assignment
    public async Task<AssignmentResult> AssignUserRolesAsync(
        int userId, 
        AssignUserRolesDto assignUserRolesDto, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Bulk assigning {GroupCount} groups and {PermissionCount} permissions to user {UserId}",
            assignUserRolesDto.GroupIds.Count,
            assignUserRolesDto.DirectPermissionIds.Count,
            userId);

        var result = new AssignmentResult();

        // Load user with groups and permissions
        var user = await _context.Users
            .Include(u => u.Groups)
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Process groups
        if (assignUserRolesDto.GroupIds.Any())
        {
            var existingGroupIds = user.Groups.Select(g => g.GroupId).ToHashSet();

            foreach (var groupId in assignUserRolesDto.GroupIds)
            {
                // Skip if already assigned
                if (existingGroupIds.Contains(groupId))
                {
                    result.SkippedGroupIds.Add(groupId);
                    _logger.LogDebug("Group {GroupId} already assigned to user {UserId}, skipping", groupId, userId);
                    continue;
                }

                // Check if group exists
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

                if (group == null)
                {
                    result.InvalidGroupIds.Add(groupId);
                    _logger.LogWarning("Group {GroupId} not found", groupId);
                    continue;
                }

                user.Groups.Add(group);
                result.GroupsAssigned++;
            }
        }

        // Process direct permissions
        if (assignUserRolesDto.DirectPermissionIds.Any())
        {
            var existingPermissionIds = user.Permissions.Select(p => p.PermissionId).ToHashSet();

            foreach (var permissionId in assignUserRolesDto.DirectPermissionIds)
            {
                // Skip if already assigned
                if (existingPermissionIds.Contains(permissionId))
                {
                    result.SkippedPermissionIds.Add(permissionId);
                    _logger.LogDebug("Permission {PermissionId} already assigned to user {UserId}, skipping", permissionId, userId);
                    continue;
                }

                // Check if permission exists
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

                if (permission == null)
                {
                    result.InvalidPermissionIds.Add(permissionId);
                    _logger.LogWarning("Permission {PermissionId} not found", permissionId);
                    continue;
                }

                user.Permissions.Add(permission);
                result.PermissionsAssigned++;
            }
        }

        // Save all changes in a single transaction
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Bulk assignment completed for user {UserId}: {GroupsAssigned} groups, {PermissionsAssigned} permissions assigned",
            userId,
            result.GroupsAssigned,
            result.PermissionsAssigned);

        return result;
    }

    public async Task<UserRolesSyncResult> SyncUserRolesAsync(
        int userId,
        SyncUserRolesDto syncUserRolesDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing roles (groups and permissions) for user {UserId}", userId);

        var result = new UserRolesSyncResult();

        // Use execution strategy to handle retry logic with transactions
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Start a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get the user with groups and direct permissions
                var user = await _context.Users
                    .Include(u => u.Groups)
                    .Include(u => u.Permissions)
                    .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found");
                }

                // ========== SYNC GROUPS ==========

                // Remove duplicates from input
                var uniqueGroupIds = syncUserRolesDto.GroupIds.Distinct().ToList();

                // Validate that all group IDs exist
                var existingGroups = await _context.Groups
                    .Where(g => uniqueGroupIds.Contains(g.GroupId))
                    .Select(g => g.GroupId)
                    .ToListAsync(cancellationToken);

                // Find invalid group IDs
                result.InvalidGroupIds = uniqueGroupIds
                    .Except(existingGroups)
                    .ToList();

                if (result.InvalidGroupIds.Any())
                {
                    _logger.LogWarning("Invalid group IDs found: {InvalidIds}",
                        string.Join(", ", result.InvalidGroupIds));
                }

                // Use only valid group IDs
                var validGroupIds = uniqueGroupIds
                    .Except(result.InvalidGroupIds)
                    .ToList();

                // Get current group IDs
                var currentGroupIds = user.Groups
                    .Select(g => g.GroupId)
                    .ToList();

                // Determine what to add and remove for groups
                var groupsToAdd = validGroupIds
                    .Except(currentGroupIds)
                    .ToList();

                var groupsToRemove = currentGroupIds
                    .Except(validGroupIds)
                    .ToList();

                var groupsUnchanged = currentGroupIds
                    .Intersect(validGroupIds)
                    .ToList();

                // Remove groups not in the new list
                if (groupsToRemove.Any())
                {
                    var groupsToRemoveEntities = user.Groups
                        .Where(g => groupsToRemove.Contains(g.GroupId))
                        .ToList();

                    foreach (var group in groupsToRemoveEntities)
                    {
                        user.Groups.Remove(group);
                    }

                    result.GroupsRemoved = groupsToRemove.Count;
                    _logger.LogInformation("Removing {Count} groups from user {UserId}",
                        groupsToRemove.Count, userId);
                }

                // Add new groups
                if (groupsToAdd.Any())
                {
                    var newGroups = await _context.Groups
                        .Where(g => groupsToAdd.Contains(g.GroupId))
                        .ToListAsync(cancellationToken);

                    foreach (var group in newGroups)
                    {
                        user.Groups.Add(group);
                    }

                    result.GroupsAdded = groupsToAdd.Count;
                    _logger.LogInformation("Adding {Count} groups to user {UserId}",
                        groupsToAdd.Count, userId);
                }

                result.GroupsUnchanged = groupsUnchanged.Count;
                result.FinalGroupIds = validGroupIds;

                // ========== SYNC DIRECT PERMISSIONS ==========

                // Remove duplicates from input
                var uniquePermissionIds = syncUserRolesDto.PermissionIds.Distinct().ToList();

                // Validate that all permission IDs exist
                var existingPermissions = await _context.Permissions
                    .Where(p => uniquePermissionIds.Contains(p.PermissionId))
                    .Select(p => p.PermissionId)
                    .ToListAsync(cancellationToken);

                // Find invalid permission IDs
                result.InvalidPermissionIds = uniquePermissionIds
                    .Except(existingPermissions)
                    .ToList();

                if (result.InvalidPermissionIds.Any())
                {
                    _logger.LogWarning("Invalid permission IDs found: {InvalidIds}",
                        string.Join(", ", result.InvalidPermissionIds));
                }

                // Use only valid permission IDs
                var validPermissionIds = uniquePermissionIds
                    .Except(result.InvalidPermissionIds)
                    .ToList();

                // Get current permission IDs
                var currentPermissionIds = user.Permissions
                    .Select(p => p.PermissionId)
                    .ToList();

                // Determine what to add and remove for permissions
                var permissionsToAdd = validPermissionIds
                    .Except(currentPermissionIds)
                    .ToList();

                var permissionsToRemove = currentPermissionIds
                    .Except(validPermissionIds)
                    .ToList();

                var permissionsUnchanged = currentPermissionIds
                    .Intersect(validPermissionIds)
                    .ToList();

                // Remove permissions not in the new list
                if (permissionsToRemove.Any())
                {
                    var permissionsToRemoveEntities = user.Permissions
                        .Where(p => permissionsToRemove.Contains(p.PermissionId))
                        .ToList();

                    foreach (var permission in permissionsToRemoveEntities)
                    {
                        user.Permissions.Remove(permission);
                    }

                    result.PermissionsRemoved = permissionsToRemove.Count;
                    _logger.LogInformation("Removing {Count} permissions from user {UserId}",
                        permissionsToRemove.Count, userId);
                }

                // Add new permissions
                if (permissionsToAdd.Any())
                {
                    var newPermissions = await _context.Permissions
                        .Where(p => permissionsToAdd.Contains(p.PermissionId))
                        .ToListAsync(cancellationToken);

                    foreach (var permission in newPermissions)
                    {
                        user.Permissions.Add(permission);
                    }

                    result.PermissionsAdded = permissionsToAdd.Count;
                    _logger.LogInformation("Adding {Count} permissions to user {UserId}",
                        permissionsToAdd.Count, userId);
                }

                result.PermissionsUnchanged = permissionsUnchanged.Count;
                result.FinalPermissionIds = validPermissionIds;

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "User {UserId} roles synced. Groups - Added: {GroupsAdded}, Removed: {GroupsRemoved}, Unchanged: {GroupsUnchanged}. " +
                    "Permissions - Added: {PermissionsAdded}, Removed: {PermissionsRemoved}, Unchanged: {PermissionsUnchanged}",
                    userId, result.GroupsAdded, result.GroupsRemoved, result.GroupsUnchanged,
                    result.PermissionsAdded, result.PermissionsRemoved, result.PermissionsUnchanged);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error syncing roles for user {UserId}", userId);
                throw;
            }
        });

        return result;
    }
}
