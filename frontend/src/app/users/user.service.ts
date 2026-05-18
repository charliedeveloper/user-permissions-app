import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PermissionDto {
  userName: string;
  permissionKey: string;
  permissionName: string;
  groupName?: string | null;
}

export interface User {
  userId: number;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  dateHired?: string;
}

export interface SaveUserDto {
  firstName: string;
  lastName: string;
}

export interface Permission {
  permissionId: number;
  permissionKey: string;
  permissionName: string;
}

export interface BulkAssignPermissionsToUserDto {
  permissionIds: number[];
}

export interface Group {
  groupId: number;
  groupName: string;
  isActive: boolean;
}

export interface BulkAssignGroupsToUserDto {
  groupIds: number[];
}

export interface GroupAssignmentResult {
  groupsAdded: number;
  groupsRemoved: number;
  groupsUnchanged: number;
  invalidGroupIds: number[];
  finalGroupIds: number[];
  success: boolean;
}

export interface BulkAssignmentResult {
  permissionsAdded: number;
  permissionsRemoved: number;
  permissionsUnchanged: number;
  invalidPermissionIds: number[];
  finalPermissionIds: number[];
  success: boolean;
}

export interface SyncUserRolesDto {
  groupIds: number[];
  permissionIds: number[];
}

export interface UserRolesSyncResult {
  groupsAdded: number;
  groupsRemoved: number;
  groupsUnchanged: number;
  invalidGroupIds: number[];
  finalGroupIds: number[];
  permissionsAdded: number;
  permissionsRemoved: number;
  permissionsUnchanged: number;
  invalidPermissionIds: number[];
  finalPermissionIds: number[];
  success: boolean;
  totalChanges: number;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly url = 'http://localhost:5275/api/Users';
  usersSignal = signal<User[] | null>([]);
  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.url);
  }

  createUser(dto: SaveUserDto): Observable<User> {
    return this.http.post<User>(this.url, dto);
  }

  updateUser(userId: number, dto: SaveUserDto): Observable<void> {
    return this.http.put<void>(`${this.url}/${userId}`, dto);
  }

  getUserPermissions(userId: number, useLinq = false): Observable<PermissionDto[]> {
    const q = useLinq ? '?useLinq=true' : '';
    const url = `${this.url}/${userId}/permissions${q}`;
    return this.http.get<PermissionDto[]>(url);
  }

  getPermissionsForUser(userId: number): Observable<Permission[]> {
    const url = `${this.url}/${userId}/permissions/assigned`;
    return this.http.get<Permission[]>(url);
  }

  updateUserPermissions(userId: number, dto: SyncUserRolesDto): Observable<UserRolesSyncResult> {
    const url = `${this.url}/${userId}/roles/sync`;
    return this.http.put<UserRolesSyncResult>(url, dto);
  }

  getGroupsForUser(userId: number): Observable<Group[]> {
    const url = `${this.url}/${userId}/groups`;
    return this.http.get<Group[]>(url);
  }

  updateUserGroups(
    userId: number,
    dto: BulkAssignGroupsToUserDto,
  ): Observable<GroupAssignmentResult> {
    const url = `${this.url}/${userId}/groups/sync`;
    return this.http.put<GroupAssignmentResult>(url, dto);
  }
}
