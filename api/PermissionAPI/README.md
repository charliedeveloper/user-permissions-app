# Permission API - .NET 10 Database-First Implementation

## ?? Project Overview

A production-ready ASP.NET Core Web API (.NET 10) using **Entity Framework Core** in a **database-first** approach with a **thin-controller/service architecture** pattern. The API manages users, groups, and permissions with support for both stored procedures and LINQ queries.

---

## ??? Database Schema

**Database Name:** `PermissionDB`  
**Server:** `CHARLIE-ALIEN`

### Core Tables

#### `dbo.Users`
```sql
CREATE TABLE dbo.Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    UserName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(256) NOT NULL UNIQUE,
    FirstName    NVARCHAR(50) NULL,
    LastName     NVARCHAR(50) NULL,
    DateHired    DATE NULL
);
```

#### `dbo.Groups`
```sql
CREATE TABLE dbo.Groups (
    GroupId      INT IDENTITY(1,1) PRIMARY KEY,
    GroupName    NVARCHAR(100) NOT NULL UNIQUE
);
```

#### `dbo.Permissions`
```sql
CREATE TABLE dbo.Permissions (
    PermissionId     INT IDENTITY(1,1) PRIMARY KEY,
    PermissionKey    NVARCHAR(100) NOT NULL UNIQUE,
    PermissionName   NVARCHAR(100) NOT NULL
);
```

### Junction Tables (Many-to-Many Relationships)

#### `dbo.UserGroups`
```sql
CREATE TABLE dbo.UserGroups (
    UserId   INT NOT NULL,
    GroupId  INT NOT NULL,
    PRIMARY KEY (UserId, GroupId),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    FOREIGN KEY (GroupId) REFERENCES dbo.Groups(GroupId)
);
```

#### `dbo.UserDirectPermissions`
```sql
CREATE TABLE dbo.UserDirectPermissions (
    UserId        INT NOT NULL,
    PermissionId  INT NOT NULL,
    PRIMARY KEY (UserId, PermissionId),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(PermissionId)
);
```

#### `dbo.GroupAndPermissions`
```sql
CREATE TABLE dbo.GroupAndPermissions (
    GroupId       INT NOT NULL,
    PermissionId  INT NOT NULL,
    PRIMARY KEY (GroupId, PermissionId),
    FOREIGN KEY (GroupId) REFERENCES dbo.Groups(GroupId),
    FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(PermissionId)
);
```

### Stored Procedure

#### `dbo.sp_GetUserPermissions`
```sql
CREATE PROCEDURE dbo.sp_GetUserPermissions
    @UserId INT
AS
BEGIN
    -- Returns all permissions for a user (direct + group-based)
    SELECT DISTINCT p.PermissionKey
    FROM dbo.Permissions p
    WHERE p.PermissionId IN (
        -- Direct permissions
        SELECT PermissionId 
        FROM dbo.UserDirectPermissions 
        WHERE UserId = @UserId
        
        UNION
        
        -- Group-based permissions
        SELECT gap.PermissionId
        FROM dbo.UserGroups ug
        INNER JOIN dbo.GroupAndPermissions gap ON ug.GroupId = gap.GroupId
        WHERE ug.UserId = @UserId
    )
    ORDER BY p.PermissionKey;
END
```

**Returns:**
- `PermissionKey` (nvarchar(100))

---

## ??? Architecture

### Design Pattern: Thin Controller / Service Layer

```
???????????????????
?   Controller    ?  ? Thin: Validation, HTTP concerns
???????????????????
         ?
         ?
???????????????????
?    Service      ?  ? Business Logic
???????????????????
         ?
         ?
???????????????????
?   DbContext     ?  ? Data Access (EF Core)
???????????????????
         ?
         ?
???????????????????
?   SQL Server    ?
???????????????????
```

### Project Structure

```
PermissionAPI/
??? Controllers/
?   ??? UsersController.cs          # Thin controllers
?   ??? WeatherForecastController.cs
??? Data/
?   ??? AppDbContext.cs             # EF Core DbContext (partial)
?   ??? Entities/                   # Scaffolded entities
?       ??? User.cs
?       ??? Group.cs
?       ??? Permission.cs
?       ??? UserGroup.cs
?       ??? UserDirectPermission.cs
?       ??? GroupAndPermission.cs
??? Models/
?   ??? DTOs/
?   ?   ??? PermissionDto.cs        # API response models
?   ??? SpResults/
?       ??? UserPermissionResult.cs # SP result mapping
??? Services/
?   ??? Interfaces/
?   ?   ??? IUserPermissionService.cs
?   ??? Implementations/
?       ??? UserPermissionService.cs # Business logic
??? Properties/
?   ??? launchSettings.json
??? Program.cs                       # Startup configuration
??? appsettings.json                # Configuration
??? PermissionAPI.csproj
??? README.md
```

---

## ?? Setup Instructions

### Prerequisites

- **.NET 10 SDK**
- **SQL Server** (accessible at `CHARLIE-ALIEN`)
- **Visual Studio 2025** or **VS Code** with C# extension
- **EF Core CLI Tools**

### Step 1: Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

### Step 2: Restore NuGet Packages

```bash
cd PermissionAPI
dotnet restore
```

### Step 3: Scaffold Database Entities

Run this command from the project directory:

```bash
dotnet ef dbcontext scaffold "Server=CHARLIE-ALIEN;Database=PermissionDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Data/Entities --context AppDbContext --context-dir Data --force --no-onconfiguring --data-annotations
```

**What this does:**
- Generates entity classes from your database tables
- Creates `AppDbContext` with DbSets
- Uses data annotations for entity configuration
- Places entities in `Data/Entities/`

### Step 4: Extend AppDbContext

After scaffolding, create or update `Data/AppDbContext.cs` to add the keyless entity:

```csharp
using Microsoft.EntityFrameworkCore;
using PermissionAPI.Models.SpResults;

namespace PermissionAPI.Data;

public partial class AppDbContext
{
    // Keyless entity for stored procedure result
    public virtual DbSet<UserPermissionResult> UserPermissionResults { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configure keyless entity for SP result
        modelBuilder.Entity<UserPermissionResult>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null); // Not mapped to any table/view
        });
    }
}
```

### Step 5: Update Connection String

Verify `appsettings.json` has the correct connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CHARLIE-ALIEN;Database=PermissionDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Step 6: Build and Run

```bash
dotnet build
dotnet run
```

The API will start at:
- **HTTPS:** `https://localhost:7098`
- **HTTP:** `http://localhost:5275`

---

## ?? API Endpoints

### Get User Permissions

**Endpoint:** `GET /api/users/{userId}/permissions`

**Parameters:**
- `userId` (path, required): Integer user ID
- `useLinq` (query, optional): Boolean to use LINQ instead of SP (default: false)

**Responses:**

**200 OK** - Success
```json
[
  {
    "permissionKey": "CREATE_USER"
  },
  {
    "permissionKey": "DELETE_USER"
  },
  {
    "permissionKey": "VIEW_REPORTS"
  }
]
```

**400 Bad Request** - Invalid user ID
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Invalid user ID",
  "status": 400,
  "detail": "User ID must be a positive integer"
}
```

**404 Not Found** - User doesn't exist
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "User not found",
  "status": 404,
  "detail": "User with ID 999 does not exist"
}
```

---

## ?? Testing Examples

### Using cURL

```bash
# Get permissions using stored procedure
curl -X GET "https://localhost:7098/api/users/1/permissions" -k

# Get permissions using LINQ
curl -X GET "https://localhost:7098/api/users/1/permissions?useLinq=true" -k

# Test with invalid user
curl -X GET "https://localhost:7098/api/users/999/permissions" -k

# Test with invalid input
curl -X GET "https://localhost:7098/api/users/-1/permissions" -k
```

### Using Swagger UI

1. Navigate to `https://localhost:7098`
2. Expand **GET /api/users/{userId}/permissions**
3. Click **Try it out**
4. Enter a `userId` (e.g., `1`)
5. Optionally check `useLinq` checkbox
6. Click **Execute**

### Using PowerShell

```powershell
# Get permissions
Invoke-RestMethod -Uri "https://localhost:7098/api/users/1/permissions" -Method GET -SkipCertificateCheck

# Get permissions with LINQ
Invoke-RestMethod -Uri "https://localhost:7098/api/users/1/permissions?useLinq=true" -Method GET -SkipCertificateCheck
```

---

## ?? Key Features

### 1. **Stored Procedure Support**
- Uses `FromSqlInterpolated` for safe parameter handling
- Mapped to keyless entity `UserPermissionResult`
- Pre-compiled execution plan for optimal performance

### 2. **LINQ Alternative**
- Pure EF Core LINQ queries
- Single database roundtrip using `UNION`
- Type-safe and refactorable
- Equivalent logic to stored procedure

### 3. **Performance Optimizations**

? **AsNoTracking()**: Disabled change tracking for read-only queries  
? **Direct Projection**: Maps to DTOs without loading navigation properties  
? **Connection Resilience**: Automatic retry on transient failures (3 attempts)  
? **Query Timeout**: 30-second timeout configured  
? **Single Query Execution**: LINQ uses UNION in one database call  

### 4. **Error Handling**

? **Input Validation**: Checks for positive integer user IDs  
? **Existence Check**: Verifies user exists before querying permissions  
? **RFC 7807 Problem Details**: Standard error response format  
? **Structured Logging**: Comprehensive logging with correlation  

### 5. **Thin Controller Pattern**

Controllers handle only:
- ? HTTP concerns (routing, status codes)
- ? Input validation
- ? Calling services
- ? Returning ActionResults

Business logic stays in services.

---

## ?? Performance Comparison

| Aspect | Stored Procedure | LINQ Query |
|--------|------------------|------------|
| **Performance** | ? Excellent (pre-compiled) | ? Very Good (optimized) |
| **Maintainability** | ?? Code outside application | ? In-code, refactorable |
| **Type Safety** | ?? Runtime errors possible | ? Compile-time checked |
| **Testing** | ?? Requires database | ? Can mock DbContext |
| **Version Control** | ?? Separate SQL scripts | ? Part of codebase |
| **Flexibility** | ?? Requires migration | ? Change and redeploy |

**Recommendation:** Use stored procedure for critical performance paths; use LINQ for maintainability and rapid development.

---

## ?? Security Considerations

### Current Implementation

? **Parameterized Queries**: Uses `FromSqlInterpolated` to prevent SQL injection  
? **Input Validation**: Validates user IDs before database access  
? **HTTPS Enforcement**: Redirects HTTP to HTTPS  
? **Connection String Security**: Should be moved to Azure Key Vault or User Secrets in production  

### Future Enhancements

- [ ] Add JWT Bearer authentication
- [ ] Implement authorization policies
- [ ] Add rate limiting
- [ ] Enable CORS with specific origins
- [ ] Implement API versioning

---

## ?? NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Swashbuckle.AspNetCore` | 7.2.0 | OpenAPI/Swagger documentation |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | Design-time EF Core tools |

---

## ??? Development Tips

### Re-scaffolding After Schema Changes

If your database schema changes, re-run the scaffold command:

```bash
dotnet ef dbcontext scaffold "Server=CHARLIE-ALIEN;Database=PermissionDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Data/Entities --context AppDbContext --context-dir Data --force --no-onconfiguring --data-annotations
```

**Important:** The `--force` flag will overwrite existing files. Make sure to preserve your custom `OnModelCreatingPartial` method.

### Logging SQL Queries

To see generated SQL in the console, update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Using User Secrets (Development)

Instead of hardcoding connection strings:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=CHARLIE-ALIEN;Database=PermissionDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

---

## ?? Additional Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Swagger/OpenAPI](https://swagger.io/specification/)
- [Problem Details (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807)

---

## ?? Contributing

This project follows:
- **Clean Architecture** principles
- **SOLID** design patterns
- **RESTful** API conventions
- **Async/Await** best practices

---

## ?? License

This is an internal API project for permission management.

---

## ?? Support

For issues or questions:
- **Email:** support@permissionapi.com
- **Team:** Permission API Team

---

**Last Updated:** 2025
**Version:** 1.0.0
**.NET Version:** 10.0
