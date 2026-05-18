# JWT Authentication Implementation Guide

## Overview
Complete JWT authentication has been implemented for the Permission API with the following features:
- User registration with password validation
- Login with email and password
- JWT access tokens (60 minutes expiration)
- Refresh tokens (7 days expiration)
- Account lockout after 5 failed login attempts (15 minutes)
- Password change functionality
- Secure password hashing using HMACSHA512

## Database Changes
The following fields were added to the Users table:
- `PasswordHash` (NVARCHAR(MAX))
- `PasswordSalt` (NVARCHAR(MAX))
- `RefreshToken` (NVARCHAR(MAX))
- `RefreshTokenExpiryTime` (DATETIME2)
- `LastLoginDate` (DATETIME2)
- `FailedLoginAttempts` (INT)
- `IsLocked` (BIT)
- `LockoutEnd` (DATETIME2)

## API Endpoints

### 1. Register a New User
**POST** `/api/Auth/register`

Request Body:
```json
{
  "userName": "john.doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

Password Requirements:
- Minimum 6 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character (@$!%*?&)

Response (201 Created):
```json
{
  "userId": 1,
  "userName": "john.doe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "dateHired": null
}
```

### 2. Login
**POST** `/api/Auth/login`

Request Body:
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

Response (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64EncodedRefreshToken==",
  "expiresAt": "2024-01-15T14:30:00Z",
  "user": {
    "userId": 1,
    "userName": "john.doe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateHired": null
  }
}
```

### 3. Refresh Access Token
**POST** `/api/Auth/refresh`

Request Body:
```json
{
  "accessToken": "expiredOrExpiringToken",
  "refreshToken": "validRefreshToken"
}
```

Response (200 OK):
```json
{
  "accessToken": "newAccessToken",
  "refreshToken": "newRefreshToken",
  "expiresAt": "2024-01-15T15:30:00Z",
  "user": { ... }
}
```

### 4. Change Password (Requires Authentication)
**POST** `/api/Auth/change-password`

Headers:
```
Authorization: Bearer {accessToken}
```

Request Body:
```json
{
  "currentPassword": "SecurePass123!",
  "newPassword": "NewSecurePass456!"
}
```

Response (204 No Content)

### 5. Logout (Requires Authentication)
**POST** `/api/Auth/logout`

Headers:
```
Authorization: Bearer {accessToken}
```

Response (204 No Content)

### 6. Get Current User Info (Requires Authentication)
**GET** `/api/Auth/me`

Headers:
```
Authorization: Bearer {accessToken}
```

Response (200 OK):
```json
{
  "userId": "1",
  "email": "john.doe@example.com",
  "userName": "john.doe",
  "claims": [
    { "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "value": "1" },
    { "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "value": "john.doe@example.com" },
    { "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "value": "john.doe" },
    { "type": "jti", "value": "unique-token-id" }
  ]
}
```

## Testing with Swagger

1. **Start the application** and navigate to the Swagger UI (usually at `http://localhost:5275`)

2. **Register a new user:**
   - Find `POST /api/Auth/register`
   - Click "Try it out"
   - Enter user details
   - Execute

3. **Login:**
   - Find `POST /api/Auth/login`
   - Click "Try it out"
   - Enter email and password
   - Execute
   - **Copy the `accessToken` from the response**

4. **Authorize in Swagger:**
   - Click the **"Authorize"** button (lock icon) at the top right
   - Enter: `Bearer {paste-your-token-here}`
   - Click "Authorize"
   - Click "Close"

5. **Now you can call protected endpoints** (those with a lock icon)

## Protecting Your Endpoints

### Protect Entire Controller
Add `[Authorize]` attribute to the controller class:

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    // All endpoints require authentication
}
```

### Protect Specific Endpoints
Add `[Authorize]` to individual actions:

```csharp
[Authorize]
[HttpPost]
public async Task<ActionResult<GroupDto>> CreateGroup(...)
{
    // Only authenticated users can create groups
}

[AllowAnonymous] // Public even if controller has [Authorize]
[HttpGet]
public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetAllGroups(...)
{
    // Anyone can view groups
}
```

### Access Current User in Controllers
```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
{
    // Use userId
}

var email = User.FindFirst(ClaimTypes.Email)?.Value;
var userName = User.FindFirst(ClaimTypes.Name)?.Value;
```

## Angular Integration

### Auth Service
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5275/api/Auth';

  constructor(private http: HttpClient) {}

  register(userData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, userData);
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem('accessToken', response.accessToken);
          localStorage.setItem('refreshToken', response.refreshToken);
          localStorage.setItem('user', JSON.stringify(response.user));
        })
      );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe();
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
```

### HTTP Interceptor
```typescript
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('accessToken');
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};
```

### Register Interceptor in app.config.ts
```typescript
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor])
    )
  ]
};
```

## Configuration Settings

### appsettings.json
```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!Change_This_In_Production",
    "Issuer": "PermissionAPI",
    "Audience": "PermissionAPIClients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Production Recommendations
1. **Secret Key:** Use a cryptographically secure random string (64+ characters)
2. **Store in Environment Variables or Azure Key Vault:**
   ```bash
   dotnet user-secrets set "JwtSettings:Secret" "your-production-secret"
   ```
3. **Enable HTTPS:** Set `RequireHttpsMetadata = true` in JWT Bearer options
4. **Use Shorter Token Expiration:** Consider 15-30 minutes for access tokens
5. **Implement Token Blacklisting:** For immediate revocation if needed

## Security Features

### Password Hashing
- Uses HMACSHA512 algorithm
- Unique salt per user
- Password hash and salt stored separately

### Account Lockout
- Locks account after 5 failed login attempts
- 15-minute lockout duration
- Automatic unlock after lockout period
- Failed attempts reset on successful login

### Token Security
- JWT tokens signed with HMACSHA256
- Refresh tokens are cryptographically random (64 bytes)
- Tokens expire (access: 60 min, refresh: 7 days)
- ClockSkew set to zero (no tolerance for expired tokens)

### Password Requirements
- Minimum 6 characters
- Must contain: uppercase, lowercase, number, special character
- Validated on both client and server side

## Error Handling

### Login Errors
- **401 Unauthorized:** Invalid credentials or account locked
- **400 Bad Request:** Missing or invalid email/password format

### Registration Errors
- **400 Bad Request:** Email already exists, weak password, or validation errors

### Token Refresh Errors
- **401 Unauthorized:** Invalid or expired refresh token

### Password Change Errors
- **401 Unauthorized:** Not authenticated
- **400 Bad Request:** Current password is incorrect

## Files Created/Modified

### New Files:
1. `Models/DTOs/Auth/LoginRequestDto.cs`
2. `Models/DTOs/Auth/LoginResponseDto.cs`
3. `Models/DTOs/Auth/RefreshTokenRequestDto.cs`
4. `Models/DTOs/Auth/RegisterUserDto.cs`
5. `Models/DTOs/Auth/ChangePasswordDto.cs`
6. `Models/Configuration/JwtSettings.cs`
7. `Services/Interfaces/IAuthService.cs`
8. `Services/Implementations/AuthService.cs`
9. `Controllers/AuthController.cs`

### Modified Files:
1. `appsettings.json` - Added JwtSettings
2. `Program.cs` - Added JWT authentication configuration
3. `Data/AppDbContext.cs` - Added using for UserPermissionResult
4. `Data/Entities/User.cs` - Re-scaffolded with new password fields

## Next Steps

1. **Test all endpoints** using Swagger
2. **Protect your controllers** by adding `[Authorize]` attributes
3. **Update Angular app** to use the authentication service
4. **Change the JWT Secret** in production
5. **Consider adding:**
   - Email verification
   - Two-factor authentication (2FA)
   - Forgot password / password reset
   - Rate limiting on login endpoint
   - Audit logging for security events
