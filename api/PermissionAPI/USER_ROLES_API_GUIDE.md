# User Roles Management - API Documentation

## ?? Overview

This implementation provides comprehensive user role management capabilities, allowing you to:
- Assign users to groups (UserGroups table)
- Assign direct permissions to users (UserDirectPermissions table)
- Perform bulk assignments in a single API call
- Manage all relationships via RESTful endpoints

---

## ?? API Endpoints

### **1?? User-Group Management (UserGroups Table)**

#### Get User's Groups
```http
GET /api/users/{userId}/groups
```

**Response Example:**
```json
[
  {
    "groupId": 1,
    "groupName": "Administrators"
  },
  {
    "groupId": 2,
    "groupName": "Developers"
  }
]
```

#### Assign User to Group
```http
POST /api/users/{userId}/groups
Content-Type: application/json

{
  "groupId": 1
}
```

**Response:** `204 No Content` on success

#### Remove User from Group
```http
DELETE /api/users/{userId}/groups/{groupId}
```

**Response:** `204 No Content` on success

---

### **2?? User Direct Permissions (UserDirectPermissions Table)**

#### Get User's Direct Permissions
```http
GET /api/users/{userId}/permissions/direct
```

**Response Example:**
```json
[
  {
    "permissionId": 1,
    "permissionKey": "DELETE_USER",
    "permissionName": "Delete User"
  },
  {
    "permissionId": 2,
    "permissionKey": "CREATE_REPORT",
    "permissionName": "Create Report"
  }
]
```

#### Assign Direct Permission to User
```http
POST /api/users/{userId}/permissions
Content-Type: application/json

{
  "permissionId": 1
}
```

**Response:** `204 No Content` on success

#### Remove Direct Permission from User
```http
DELETE /api/users/{userId}/permissions/{permissionId}
```

**Response:** `204 No Content` on success

---

### **3?? Bulk Assignment (? Recommended for UI)**

#### Assign Multiple Groups and Permissions
```http
POST /api/users/{userId}/roles
Content-Type: application/json

{
  "groupIds": [1, 2, 3],
  "directPermissionIds": [5, 6, 7]
}
```

**Response Example:**
```json
{
  "groupsAssigned": 2,
  "permissionsAssigned": 3,
  "skippedGroupIds": [1],        // Already assigned
  "skippedPermissionIds": [],
  "invalidGroupIds": [],
  "invalidPermissionIds": [],
  "success": true
}
```

**Benefits:**
- ? Single API call for both groups and permissions
- ? Atomic transaction (all or nothing)
- ? Detailed feedback on what succeeded/failed
- ? Idempotent (safe to retry)

---

## ?? Angular Integration Examples

### Service Implementation

```typescript
// user-role.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AssignUserRolesRequest {
  groupIds: number[];
  directPermissionIds: number[];
}

export interface AssignmentResult {
  groupsAssigned: number;
  permissionsAssigned: number;
  skippedGroupIds: number[];
  skippedPermissionIds: number[];
  invalidGroupIds: number[];
  invalidPermissionIds: number[];
  success: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserRoleService {
  private apiUrl = 'https://localhost:7098/api/users';

  constructor(private http: HttpClient) {}

  // Bulk assignment (recommended)
  assignUserRoles(
    userId: number, 
    groupIds: number[], 
    permissionIds: number[]
  ): Observable<AssignmentResult> {
    return this.http.post<AssignmentResult>(
      `${this.apiUrl}/${userId}/roles`,
      {
        groupIds: groupIds,
        directPermissionIds: permissionIds
      }
    );
  }

  // Individual operations
  assignUserToGroup(userId: number, groupId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${userId}/groups`,
      { groupId }
    );
  }

  removeUserFromGroup(userId: number, groupId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${userId}/groups/${groupId}`
    );
  }

  assignPermissionToUser(userId: number, permissionId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${userId}/permissions`,
      { permissionId }
    );
  }

  removePermissionFromUser(userId: number, permissionId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${userId}/permissions/${permissionId}`
    );
  }

  getUserGroups(userId: number): Observable<GroupDto[]> {
    return this.http.get<GroupDto[]>(`${this.apiUrl}/${userId}/groups`);
  }

  getUserDirectPermissions(userId: number): Observable<PermissionDetailDto[]> {
    return this.http.get<PermissionDetailDto[]>(
      `${this.apiUrl}/${userId}/permissions/direct`
    );
  }
}
```

### Component Example

```typescript
// user-role-editor.component.ts
import { Component, OnInit } from '@angular/core';
import { UserRoleService } from './user-role.service';

@Component({
  selector: 'app-user-role-editor',
  templateUrl: './user-role-editor.component.html'
})
export class UserRoleEditorComponent implements OnInit {
  userId = 1;
  selectedGroupIds: number[] = [];
  selectedPermissionIds: number[] = [];
  assignmentResult: AssignmentResult | null = null;

  constructor(private userRoleService: UserRoleService) {}

  saveUserRoles(): void {
    this.userRoleService
      .assignUserRoles(this.userId, this.selectedGroupIds, this.selectedPermissionIds)
      .subscribe({
        next: (result) => {
          this.assignmentResult = result;
          
          if (result.success) {
            console.log('All assignments successful!');
            console.log(`Assigned ${result.groupsAssigned} groups`);
            console.log(`Assigned ${result.permissionsAssigned} permissions`);
          } else {
            console.warn('Some assignments failed:');
            console.warn('Invalid groups:', result.invalidGroupIds);
            console.warn('Invalid permissions:', result.invalidPermissionIds);
          }

          if (result.skippedGroupIds.length > 0) {
            console.info('Already assigned groups:', result.skippedGroupIds);
          }
        },
        error: (err) => console.error('Error assigning roles:', err)
      });
  }
}
```

### Template Example

```html
<!-- user-role-editor.component.html -->
<div class="user-role-editor">
  <h3>Assign Roles to User {{ userId }}</h3>

  <div class="groups-selection">
    <h4>Groups</h4>
    <mat-selection-list [(ngModel)]="selectedGroupIds">
      <mat-list-option *ngFor="let group of availableGroups" [value]="group.groupId">
        {{ group.groupName }}
      </mat-list-option>
    </mat-selection-list>
  </div>

  <div class="permissions-selection">
    <h4>Direct Permissions</h4>
    <mat-selection-list [(ngModel)]="selectedPermissionIds">
      <mat-list-option *ngFor="let perm of availablePermissions" [value]="perm.permissionId">
        {{ perm.permissionName }}
      </mat-list-option>
    </mat-selection-list>
  </div>

  <button mat-raised-button color="primary" (click)="saveUserRoles()">
    Save Roles
  </button>

  <!-- Result display -->
  <div *ngIf="assignmentResult" class="result">
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon [color]="assignmentResult.success ? 'primary' : 'warn'">
            {{ assignmentResult.success ? 'check_circle' : 'warning' }}
          </mat-icon>
          Assignment Result
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <p>Groups Assigned: {{ assignmentResult.groupsAssigned }}</p>
        <p>Permissions Assigned: {{ assignmentResult.permissionsAssigned }}</p>
        
        <div *ngIf="assignmentResult.skippedGroupIds.length > 0" class="info">
          <p>Already assigned groups: {{ assignmentResult.skippedGroupIds.join(', ') }}</p>
        </div>
        
        <div *ngIf="!assignmentResult.success" class="warning">
          <p *ngIf="assignmentResult.invalidGroupIds.length > 0">
            Invalid groups: {{ assignmentResult.invalidGroupIds.join(', ') }}
          </p>
          <p *ngIf="assignmentResult.invalidPermissionIds.length > 0">
            Invalid permissions: {{ assignmentResult.invalidPermissionIds.join(', ') }}
          </p>
        </div>
      </mat-card-content>
    </mat-card>
  </div>
</div>
```

---

## ?? Complete Workflow Example

### Scenario: User Management Form

```typescript
// Complete workflow for managing user roles
class UserManagementWorkflow {
  constructor(private userRoleService: UserRoleService) {}

  async setupUserRoles(userId: number) {
    // 1. Load current assignments
    const [currentGroups, currentPermissions] = await Promise.all([
      this.userRoleService.getUserGroups(userId).toPromise(),
      this.userRoleService.getUserDirectPermissions(userId).toPromise()
    ]);

    console.log('Current Groups:', currentGroups);
    console.log('Current Direct Permissions:', currentPermissions);

    // 2. User selects new roles in UI
    const selectedGroupIds = [1, 2, 3];
    const selectedPermissionIds = [5, 6];

    // 3. Bulk assign new roles
    const result = await this.userRoleService
      .assignUserRoles(userId, selectedGroupIds, selectedPermissionIds)
      .toPromise();

    // 4. Handle result
    if (result.success) {
      console.log('? All roles assigned successfully!');
    } else {
      console.log('?? Some assignments failed');
      // Show user which IDs were invalid
      this.showErrorMessage(result);
    }
  }

  showErrorMessage(result: AssignmentResult) {
    const errors = [];
    
    if (result.invalidGroupIds.length > 0) {
      errors.push(`Invalid groups: ${result.invalidGroupIds.join(', ')}`);
    }
    
    if (result.invalidPermissionIds.length > 0) {
      errors.push(`Invalid permissions: ${result.invalidPermissionIds.join(', ')}`);
    }
    
    // Display to user
    alert(errors.join('\n'));
  }
}
```

---

## ?? Database Tables Affected

| API Endpoint | Database Table | Operation |
|-------------|----------------|-----------|
| `POST /api/users/{id}/groups` | **UserGroups** | INSERT |
| `DELETE /api/users/{id}/groups/{groupId}` | **UserGroups** | DELETE |
| `POST /api/users/{id}/permissions` | **UserDirectPermissions** | INSERT |
| `DELETE /api/users/{id}/permissions/{permId}` | **UserDirectPermissions** | DELETE |
| `POST /api/users/{id}/roles` | **UserGroups** + **UserDirectPermissions** | INSERT (batch) |

---

## ? Testing Examples

### cURL Commands

```bash
# Assign user to group
curl -X POST "https://localhost:7098/api/users/1/groups" \
  -H "Content-Type: application/json" \
  -d '{"groupId": 1}' -k

# Assign direct permission
curl -X POST "https://localhost:7098/api/users/1/permissions" \
  -H "Content-Type: application/json" \
  -d '{"permissionId": 3}' -k

# Bulk assignment
curl -X POST "https://localhost:7098/api/users/1/roles" \
  -H "Content-Type: application/json" \
  -d '{"groupIds": [1,2], "directPermissionIds": [3,4]}' -k

# Get user groups
curl -X GET "https://localhost:7098/api/users/1/groups" -k

# Get user direct permissions
curl -X GET "https://localhost:7098/api/users/1/permissions/direct" -k

# Remove user from group
curl -X DELETE "https://localhost:7098/api/users/1/groups/1" -k

# Remove direct permission
curl -X DELETE "https://localhost:7098/api/users/1/permissions/3" -k
```

### PowerShell Commands

```powershell
# Bulk assignment
$body = @{
    groupIds = @(1, 2, 3)
    directPermissionIds = @(5, 6, 7)
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7098/api/users/1/roles" `
    -Method POST `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

# Get user groups
Invoke-RestMethod -Uri "https://localhost:7098/api/users/1/groups" `
    -Method GET `
    -SkipCertificateCheck
```

---

## ?? Best Practices

### ? Recommended Approach

**Use the bulk endpoint (`POST /api/users/{id}/roles`) when:**
- User is filling out a form with multiple selections
- You want atomic operations (all succeed or none)
- You need detailed feedback on what worked/failed

**Use individual endpoints when:**
- Adding/removing single items dynamically
- User is making incremental changes
- You want fine-grained control

### ?? Error Handling

```typescript
this.userRoleService.assignUserRoles(userId, groupIds, permIds)
  .subscribe({
    next: (result) => {
      if (result.success) {
        this.showSuccessMessage();
      } else {
        // Partial failure - some assignments worked
        this.showPartialSuccessWarning(result);
      }
    },
    error: (err) => {
      // Complete failure - user not found or validation error
      if (err.status === 404) {
        this.showUserNotFoundError();
      } else if (err.status === 400) {
        this.showValidationError(err.error);
      }
    }
  });
```

---

## ?? Complete Feature Matrix

| Feature | Endpoint | Method | Bulk Support |
|---------|----------|--------|--------------|
| Get user's groups | `/api/users/{id}/groups` | GET | ? |
| Assign user to group | `/api/users/{id}/groups` | POST | ? via `/roles` |
| Remove user from group | `/api/users/{id}/groups/{groupId}` | DELETE | ? |
| Get direct permissions | `/api/users/{id}/permissions/direct` | GET | ? |
| Assign direct permission | `/api/users/{id}/permissions` | POST | ? via `/roles` |
| Remove direct permission | `/api/users/{id}/permissions/{permId}` | DELETE | ? |
| Bulk assign roles | `/api/users/{id}/roles` | POST | ? |

---

## ?? Quick Start Checklist

- [x] DTOs created
- [x] Service layer implemented
- [x] Controller endpoints created
- [x] Build successful
- [x] Ready for Angular integration

**Next Steps:**
1. Run the application: `dotnet run`
2. Test endpoints in Swagger UI: `https://localhost:7098`
3. Integrate with your Angular UI
4. Enjoy comprehensive user role management! ??
