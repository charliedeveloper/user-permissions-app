import { Component, Input, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { UserService, PermissionDto, User } from './user.service';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AssignUserPermissionsPanelComponent } from './assign-user-permissions-panel.component';
import { PermissionTreeComponent } from './permission-tree.component';

@Component({
  selector: 'user-permissions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatListModule,
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    AssignUserPermissionsPanelComponent,
    PermissionTreeComponent,
  ],
  templateUrl: './user-permissions.component.html',
  styleUrls: ['./user-permissions.component.scss'],
})
export class UserPermissionsComponent implements OnChanges {
  @Input() userId: number | null = null;
  @Input() useLinq = false;

  permissions: PermissionDto[] = [];
  allPermissions: PermissionDto[] = [];
  filterText = '';
  sortField: 'permissionName' | 'groupName' | null = 'permissionName';
  sortDirection: 'asc' | 'desc' | null = 'asc';
  loading = false;
  error = '';
  displayedColumns = ['permissionName', 'permissionKey', 'groupName'];
  showAssignPanel = false;
  currentUser: User | null = null;
  viewMode: 'list' | 'tree' = 'list';

  constructor(
    private svc: UserService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['userId'] && this.userId != null) {
      this.filterText = '';
      this.loadPermissions();
      this.loadUserDetails();
      this.viewMode = 'list';
    }
  }

  setView(mode: 'list' | 'tree') {
    this.viewMode = mode;
  }

  applyFilter() {
    const query = (this.filterText || '').toLowerCase();
    if (!query) {
      this.permissions = this.allPermissions;
    } else {
      this.permissions = this.allPermissions.filter((p) =>
        (p.permissionName || '').toLowerCase().includes(query),
      );
    }
    this.applySorting();
  }

  onFilterChange() {
    this.applyFilter();
  }

  onSort(field: 'permissionName' | 'groupName') {
    if (this.sortField === field) {
      if (this.sortDirection === 'asc') {
        this.sortDirection = 'desc';
      } else if (this.sortDirection === 'desc') {
        this.sortField = null;
        this.sortDirection = null;
      }
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.applySorting();
  }

  private applySorting() {
    if (!this.sortField || !this.sortDirection) {
      return;
    }

    this.permissions = [...this.permissions].sort((a, b) => {
      let aVal: string | undefined;
      let bVal: string | undefined;

      if (this.sortField === 'permissionName') {
        aVal = a.permissionName || '';
        bVal = b.permissionName || '';
      } else if (this.sortField === 'groupName') {
        aVal = a.groupName || '';
        bVal = b.groupName || '';
      }

      if (!aVal || !bVal) return 0;

      const comparison = aVal.localeCompare(bVal);
      return this.sortDirection === 'asc' ? comparison : -comparison;
    });
  }

  private loadUserDetails() {
    if (this.userId == null) return;
    this.svc.getUsers().subscribe({
      next: (users) => {
        this.currentUser = users.find((u) => u.userId === this.userId) || null;
      },
      error: (err) => {
        console.error('Failed to load user details', err);
      },
    });
  }

  onAssignPermission() {
    if (this.currentUser) {
      this.showAssignPanel = true;
    }
  }

  onPanelClose(refreshNeeded: boolean) {
    this.showAssignPanel = false;
    if (refreshNeeded) {
      this.loadPermissions();
    }
  }

  private loadPermissions() {
    if (this.userId == null) return;
    this.loading = true;
    this.error = '';
    this.svc.getUserPermissions(this.userId, this.useLinq).subscribe({
      next: (p) => {
        Promise.resolve().then(() => {
          this.allPermissions = p || [];
          this.applyFilter();
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        console.error('Get user permissions failed', err);
        Promise.resolve().then(() => {
          this.permissions = [];
          this.loading = false;
          this.error = 'Failed to load permissions';
          this.cdr.detectChanges();
        });
      },
    });
  }
}
