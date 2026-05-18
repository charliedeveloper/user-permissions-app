using Microsoft.EntityFrameworkCore;
using PermissionAPI.Data;
using PermissionAPI.Data.Entities;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Services.Implementations;

/// <summary>
/// Implementation of permission service
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(AppDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PermissionDetailDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all permissions");

        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.PermissionKey)
            .Select(p => new PermissionDetailDto
            {
                PermissionId = p.PermissionId,
                PermissionKey = p.PermissionKey,
                PermissionName = p.PermissionName,
                IsActive = p.IsActive ?? true
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} permissions", permissions.Count);
        return permissions.AsReadOnly();
    }

    public async Task<PermissionDetailDto?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching permission with ID {PermissionId}", permissionId);

        var permission = await _context.Permissions
            .AsNoTracking()
            .Where(p => p.PermissionId == permissionId)
            .Select(p => new PermissionDetailDto
            {
                PermissionId = p.PermissionId,
                PermissionKey = p.PermissionKey,
                PermissionName = p.PermissionName,
                IsActive = p.IsActive ?? true
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission with ID {PermissionId} not found", permissionId);
        }

        return permission;
    }

    public async Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new permission: {PermissionKey}", createPermissionDto.PermissionKey);

        var permission = new Permission
        {
            PermissionKey = createPermissionDto.PermissionKey,
            PermissionName = createPermissionDto.PermissionName,
            IsActive = createPermissionDto.IsActive
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission created successfully with ID {PermissionId}", permission.PermissionId);

        return new PermissionDetailDto
        {
            PermissionId = permission.PermissionId,
            PermissionKey = permission.PermissionKey,
            PermissionName = permission.PermissionName,
            IsActive = permission.IsActive ?? true
        };
    }

    public async Task<bool> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating permission {PermissionId}", permissionId);

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found for update", permissionId);
            return false;
        }

        permission.PermissionKey = updatePermissionDto.PermissionKey;
        permission.PermissionName = updatePermissionDto.PermissionName;
        permission.IsActive = updatePermissionDto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} updated successfully", permissionId);
        return true;
    }

    public async Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting permission {PermissionId}", permissionId);

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found for deletion", permissionId);
            return false;
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} deleted successfully", permissionId);
        return true;
    }

    public async Task<bool> PermissionExistsAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .AnyAsync(p => p.PermissionId == permissionId, cancellationToken);
    }
}
