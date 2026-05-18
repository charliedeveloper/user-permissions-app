using PermissionAPI.Models.DTOs;
using PermissionAPI.Models.DTOs.Auth;

namespace PermissionAPI.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns tokens
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with tokens</returns>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="registerDto">Registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user DTO</returns>
    Task<UserDto> RegisterAsync(RegisterUserDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="refreshRequest">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New login response with tokens</returns>
    Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto refreshRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's password
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="changePasswordDto">Password change data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> LogoutAsync(int userId, CancellationToken cancellationToken = default);
}
