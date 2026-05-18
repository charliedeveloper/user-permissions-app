using Microsoft.EntityFrameworkCore;
using PermissionAPI.Data;
using PermissionAPI.Data.Entities;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Services.Implementations;

/// <summary>
/// Implementation of group service
/// </summary>
public class GroupService : IGroupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<GroupService> _logger;

    public GroupService(AppDbContext context, ILogger<GroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GroupDto>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all groups");

        var groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.GroupName)
            .Select(g => new GroupDto
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                IsActive = g.IsActive ?? true
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} groups", groups.Count);
        return groups.AsReadOnly();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching group with ID {GroupId}", groupId);
        var group = await _context.Groups
            .AsNoTracking()
            .Where(g => g.GroupId == groupId)
            .Select(g => new GroupDto
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                IsActive = g.IsActive ?? true
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group with ID {GroupId} not found", groupId);
        }

        return group;
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new group: {GroupName}", createGroupDto.GroupName);

        var group = new Group
        {
            GroupName = createGroupDto.GroupName,
            IsActive = createGroupDto.IsActive
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Group created successfully with ID {GroupId}", group.GroupId);

        return new GroupDto
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            IsActive = group.IsActive ?? true
        };
    }

    public async Task<bool> UpdateGroupAsync(int groupId, UpdateGroupDto updateGroupDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating group {GroupId}", groupId);

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found for update", groupId);
            return false;
        }

        group.GroupName = updateGroupDto.GroupName;
        group.IsActive = updateGroupDto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Group {GroupId} updated successfully", groupId);
        return true;
    }

    public async Task<bool> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting group {GroupId}", groupId);

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found for deletion", groupId);
            return false;
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Group {GroupId} deleted successfully", groupId);
        return true;
    }

    public async Task<bool> GroupExistsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .AsNoTracking()
            .AnyAsync(g => g.GroupId == groupId, cancellationToken);
    }

    public async Task<bool> AssignPermissionToGroupAsync(int groupId, int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning permission {PermissionId} to group {GroupId}", permissionId, groupId);

        // Check if the relationship already exists
        var exists = await _context.Groups
            .AsNoTracking()
            .Where(g => g.GroupId == groupId)
            .SelectMany(g => g.Permissions)
            .AnyAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Permission {PermissionId} is already assigned to group {GroupId}", permissionId, groupId);
            return false;
        }

        // Load the group with its permissions
        var group = await _context.Groups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return false;
        }

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found", permissionId);
            return false;
        }

        group.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} assigned to group {GroupId} successfully", permissionId, groupId);
        return true;
    }

    public async Task<bool> RemovePermissionFromGroupAsync(int groupId, int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing permission {PermissionId} from group {GroupId}", permissionId, groupId);

        var group = await _context.Groups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return false;
        }

        var permission = group.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found in group {GroupId}", permissionId, groupId);
            return false;
        }

        group.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} removed from group {GroupId} successfully", permissionId, groupId);
        return true;
    }

    public async Task<IReadOnlyList<PermissionDetailDto>> GetGroupPermissionsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching permissions for group {GroupId}", groupId);

        var permissions = await _context.Groups
            .AsNoTracking()
            .Where(g => g.GroupId == groupId)
            .SelectMany(g => g.Permissions)
            .OrderBy(p => p.PermissionKey)
            .Select(p => new PermissionDetailDto
            {
                PermissionId = p.PermissionId,
                PermissionKey = p.PermissionKey,
                PermissionName = p.PermissionName,
                IsActive = p.IsActive ?? true
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} permissions for group {GroupId}", permissions.Count, groupId);
        return permissions.AsReadOnly();
    }

    public async Task<IReadOnlyList<GroupWithPermissionCountDto>> GetGroupsWithPermissionCountAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all groups with permission counts");

        var groupsWithCounts = await _context.Groups
            .AsNoTracking()
            .Select(g => new GroupWithPermissionCountDto
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                IsActive = g.IsActive ?? true,
                PermissionCount = g.Permissions.Count
            })
            .OrderBy(g => g.GroupName)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} groups with permission counts", groupsWithCounts.Count);
        return groupsWithCounts.AsReadOnly();
    }

    public async Task<BulkAssignmentResult> SyncGroupPermissionsAsync(
        int groupId, 
        List<int> permissionIds, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing permissions for group {GroupId}", groupId);

        var result = new BulkAssignmentResult();

        // Use execution strategy to handle retry logic with transactions
        var strategy = _context.Database.CreateExecutionStrategy();
        
        await strategy.ExecuteAsync(async () =>
        {
            // Start a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get the group with its current permissions
                var group = await _context.Groups
                    .Include(g => g.Permissions)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

                if (group == null)
                {
                    throw new InvalidOperationException($"Group with ID {groupId} not found");
                }

                // Remove duplicates from input
                var uniquePermissionIds = permissionIds.Distinct().ToList();

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
                    // Don't throw - continue with valid IDs only
                }

                // Use only valid permission IDs
                var validPermissionIds = uniquePermissionIds
                    .Except(result.InvalidPermissionIds)
                    .ToList();

                // Get current permission IDs
                var currentPermissionIds = group.Permissions
                    .Select(p => p.PermissionId)
                    .ToList();

                // Determine what to add and remove
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
                    var permissionsToRemoveEntities = group.Permissions
                        .Where(p => permissionsToRemove.Contains(p.PermissionId))
                        .ToList();

                    foreach (var permission in permissionsToRemoveEntities)
                    {
                        group.Permissions.Remove(permission);
                    }

                    result.PermissionsRemoved = permissionsToRemove.Count;
                    _logger.LogInformation("Removing {Count} permissions from group {GroupId}", 
                        permissionsToRemove.Count, groupId);
                }

                // Add new permissions
                if (permissionsToAdd.Any())
                {
                    var newPermissions = await _context.Permissions
                        .Where(p => permissionsToAdd.Contains(p.PermissionId))
                        .ToListAsync(cancellationToken);

                    foreach (var permission in newPermissions)
                    {
                        group.Permissions.Add(permission);
                    }

                    result.PermissionsAdded = permissionsToAdd.Count;
                    _logger.LogInformation("Adding {Count} permissions to group {GroupId}", 
                        permissionsToAdd.Count, groupId);
                }

                result.PermissionsUnchanged = permissionsUnchanged.Count;
                result.FinalPermissionIds = validPermissionIds;

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Group {GroupId} permissions synced. Added: {Added}, Removed: {Removed}, Unchanged: {Unchanged}", 
                    groupId, result.PermissionsAdded, result.PermissionsRemoved, result.PermissionsUnchanged);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error syncing permissions for group {GroupId}", groupId);
                throw;
            }
        });

        return result;
    }
}
