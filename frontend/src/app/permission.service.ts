import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Permission } from './permission.component';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly url = 'http://localhost:5275/api/Permissions';
  permissionsSignal = signal<Permission[] | null>([]);
  constructor(private http: HttpClient) {}

  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.url);
  }

  updatePermission(
    permissionId: number,
    dto: { PermissionKey: string; PermissionName: string; IsActive?: boolean },
  ): Observable<Permission> {
    return this.http.put<Permission>(`${this.url}/${permissionId}`, dto);
  }

  createPermission(dto: {
    PermissionKey: string;
    PermissionName: string;
    IsActive?: boolean;
  }): Observable<Permission> {
    return this.http.post<Permission>(this.url, dto);
  }

  deactivatePermission(permissionId: number): Observable<Permission> {
    return this.http.put<Permission>(`${this.url}/${permissionId}`, {
      PermissionKey: '',
      PermissionName: '',
      IsActive: false,
    });
  }

  deletePermission(permissionId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${permissionId}`);
  }
}
