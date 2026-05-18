import { AfterViewInit, ChangeDetectorRef, Component, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionService } from './permission.service';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { EditPermissionComponent } from './edit-permission.component';
import { ConfirmDialogComponent } from './confirm-dialog.component';

export interface Permission {
  permissionId: number;
  permissionKey: string;
  permissionName: string;
  isActive?: boolean;
}

@Component({
  selector: 'permission-grid',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatInputModule,
    MatFormFieldModule,
  ],
  templateUrl: './permission.component.html',
  styleUrls: ['./permission.component.scss'],
})
export class PermissionComponent implements OnInit, AfterViewInit {
  permissions: Permission[] = [];
  allPermissions: Permission[] = [];
  filterText = '';
  displayedColumns = ['permissionId', 'permissionKey', 'permissionName', 'actions'];

  constructor(
    private svc: PermissionService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
  ) {
    effect(() => {
      const sig = this.svc.permissionsSignal();
      if (sig !== undefined) {
        Promise.resolve().then(() => {
          this.allPermissions = sig || [];
          this.applyFilter();
          this.cdr.detectChanges();
        });
      }
    });
  }

  ngOnInit(): void {
    this.svc.getPermissions().subscribe({
      next: (p) => {
        this.svc.permissionsSignal.set(p || []);
      },
      error: () => {
        this.svc.permissionsSignal.set([]);
      },
    });
  }

  ngAfterViewInit(): void {
    const sig = this.svc.permissionsSignal();
    if (sig !== undefined) {
      this.allPermissions = sig || [];
      this.applyFilter();
      this.cdr.detectChanges();
    }
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
  }

  onFilterChange() {
    this.applyFilter();
  }

  edit(p: Permission) {
    const ref = this.dialog.open(EditPermissionComponent, {
      data: {
        permissionId: p.permissionId,
        permissionKey: p.permissionKey,
        permissionName: p.permissionName,
        isActive: p.isActive ?? true,
      },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      // result contains PermissionKey, PermissionName, IsActive
      this.svc
        .updatePermission(p.permissionId, {
          PermissionKey: result.PermissionKey,
          PermissionName: result.PermissionName,
          IsActive: result.IsActive,
        })
        .subscribe({
          next: () =>
            this.svc.getPermissions().subscribe({
              next: (p) => this.svc.permissionsSignal.set(p || []),
              error: () => this.svc.permissionsSignal.set([]),
            }),
          error: (err) => console.error('Update failed', err),
        });
    });
  }

  deactivate(p: Permission) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Permission',
        message: `Are you sure you want to delete permission "${p.permissionName}"? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.svc.deletePermission(p.permissionId).subscribe({
        next: () =>
          this.svc.getPermissions().subscribe({
            next: (p) => this.svc.permissionsSignal.set(p || []),
            error: () => this.svc.permissionsSignal.set([]),
          }),
        error: (err) => {
          console.error('Delete failed', err);
          // fallback: mark inactive locally
          p.isActive = false;
        },
      });
    });
  }

  onAddPermission() {
    const ref = this.dialog.open(EditPermissionComponent, {
      data: {
        permissionId: 0,
        permissionKey: '',
        permissionName: '',
        isActive: true,
      },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.svc
        .createPermission({
          PermissionKey: result.PermissionKey,
          PermissionName: result.PermissionName,
          IsActive: result.IsActive,
        })
        .subscribe({
          next: () =>
            this.svc.getPermissions().subscribe({
              next: (p) => this.svc.permissionsSignal.set(p || []),
              error: () => this.svc.permissionsSignal.set([]),
            }),
          error: (err) => console.error('Create failed', err),
        });
    });
  }
}
