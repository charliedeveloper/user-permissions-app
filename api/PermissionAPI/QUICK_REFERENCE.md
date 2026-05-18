# Permission API - Quick Reference Guide

## ?? Running the Application

```bash
cd PermissionAPI
dotnet run
```

Access Swagger UI: **https://localhost:7098**

---

## ?? API Endpoints Quick Reference

### ?? Users
```
GET    /api/users                          # List all users
GET    /api/users/{id}                     # Get user by ID
GET    /api/users/{id}/permissions         # Get user permissions
```

### ?? Groups
```
GET    /api/groups                         # List all groups
GET    /api/groups/{id}                    # Get group by ID
POST   /api/groups                         # Create group
PUT    /api/groups/{id}                    # Update group
DELETE /api/groups/{id}                    # Delete group
```

### ?? Permissions
```
GET    /api/permissions                    # List all permissions
GET    /api/permissions/{id}               # Get permission by ID
POST   /api/permissions                    # Create permission
PUT    /api/permissions/{id}               # Update permission
DELETE /api/permissions/{id}               # Delete permission
```

### ?? Group-Permission Associations
```
GET    /api/groups/{id}/permissions              # List group permissions
POST   /api/groups/{id}/permissions              # Assign permission to group
DELETE /api/groups/{id}/permissions/{permId}     # Remove permission from group
```

---

## ?? Common Request Examples

### Create Group
```bash
curl -X POST "https://localhost:7098/api/groups" \
  -H "Content-Type: application/json" \
  -d '{"groupName":"Administrators"}' -k
```

### Create Permission
```bash
curl -X POST "https://localhost:7098/api/permissions" \
  -H "Content-Type: application/json" \
  -d '{"permissionKey":"CREATE_USER","permissionName":"Create User"}' -k
```

### Assign Permission to Group
```bash
curl -X POST "https://localhost:7098/api/groups/1/permissions" \
  -H "Content-Type: application/json" \
  -d '{"permissionId":1}' -k
```

---

## ?? HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Update/Delete successful |
| 400 | Bad Request - Invalid input |
| 404 | Not Found - Resource doesn't exist |

---

## ?? Project Structure

```
PermissionAPI/
??? Controllers/
?   ??? UsersController.cs          # User endpoints
?   ??? GroupsController.cs         # Group endpoints (NEW)
?   ??? PermissionsController.cs    # Permission endpoints (NEW)
??? Services/
?   ??? Interfaces/
?   ?   ??? IUserService.cs
?   ?   ??? IGroupService.cs
?   ?   ??? IPermissionService.cs
?   ?   ??? IUserPermissionService.cs
?   ??? Implementations/
?       ??? UserService.cs
?       ??? GroupService.cs
?       ??? PermissionService.cs
?       ??? UserPermissionService.cs
??? Models/DTOs/
?   ??? UserDto.cs
?   ??? GroupDto.cs
?   ??? PermissionDto.cs
?   ??? PermissionDetailDto.cs
?   ??? CreateGroupDto.cs
?   ??? UpdateGroupDto.cs
?   ??? CreatePermissionDto.cs
?   ??? UpdatePermissionDto.cs
?   ??? AssignPermissionToGroupDto.cs
??? Data/
?   ??? AppDbContext.cs
?   ??? Entities/
?       ??? User.cs
?       ??? Group.cs
?       ??? Permission.cs
??? Program.cs
```

---

## ? All Requirements Completed

1. ? GET list of users
2. ? GET list of permissions
3. ? GET list of groups
4. ? POST, PUT, DELETE for groups
5. ? POST, PUT, DELETE for permissions
6. ? POST, DELETE for group-permission associations

---

## ?? Key Features

- ? RESTful API design
- ? Thin controller pattern
- ? Service layer architecture
- ? Input validation
- ? Error handling (RFC 7807)
- ? Swagger documentation
- ? Async/await throughout
- ? Logging
- ? EF Core with SQL Server
- ? Database-first approach

---

**Build Status:** ? SUCCESS
