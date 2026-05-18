import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Permission } from '../../permission.component';

export interface GroupWithPermissionCountDto {
  groupId: number;
  groupName: string;
  isActive: boolean;
  permissionCount: number;
}

export interface CreateGroupDto {
  GroupName: string;
  IsActive?: boolean;
}

export interface GroupDto {
  groupId: number;
  groupName: string;
  isActive: boolean;
}

// DTO sent to the server when syncing permissions for a group
export interface BulkAssignPermissionsToGroupDto {
  permissionIds: number[];
}

// Result returned from the server after syncing permissions
export interface BulkAssignmentResult {
  permissionsAdded: number;
  permissionsRemoved: number;
  permissionsUnchanged: number;
  invalidPermissionIds: number[];
  finalPermissionIds: number[];
  success: boolean;
}

@Injectable({ providedIn: 'root' })
export class ManageGroupService {
  groupsSignal = signal<GroupWithPermissionCountDto[] | null>([]);
  private baseUrl = 'http://localhost:5275/api/Groups';

  constructor(private http: HttpClient) {}

  getGroupsWithPermissionCounts(): Observable<GroupWithPermissionCountDto[]> {
    const url = `${this.baseUrl}/with-permission-counts`;
    return this.http.get<GroupWithPermissionCountDto[]>(url);
  }

  updateGroup(groupId: number, update: { GroupName: string }): Observable<void> {
    const url = `${this.baseUrl}/${groupId}`;
    return this.http.put<void>(url, update);
  }

  createGroup(create: CreateGroupDto): Observable<GroupDto> {
    const url = `${this.baseUrl}`;
    return this.http.post<GroupDto>(url, create);
  }

  // New: get permissions assigned to a group
  getPermissionsForGroup(groupId: number): Observable<Permission[]> {
    const url = `${this.baseUrl}/${groupId}/permissions`;
    return this.http.get<Permission[]>(url);
  }

  // New: sync permissions for a group using the bulk-sync endpoint
  updateGroupPermissions(groupId: number, dto: BulkAssignPermissionsToGroupDto): Observable<BulkAssignmentResult> {
    const url = `${this.baseUrl}/${groupId}/permissions/sync`;
    return this.http.put<BulkAssignmentResult>(url, dto);
  }
}
