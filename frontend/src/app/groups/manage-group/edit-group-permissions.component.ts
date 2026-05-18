import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ManageGroupService } from './manage-group.service';
import { PermissionService } from '../../permission.service';
import { Permission } from '../../permission.component';

export interface EditGroupPermissionsData {
  groupId: number;
  groupName: string;
}

@Component({
  selector: 'edit-group-permissions-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    FormsModule,
    MatCheckboxModule,
    MatListModule,
    MatSlideToggleModule,
  ],
  templateUrl: './edit-group-permissions.component.html',
  styleUrls: ['./edit-group-permissions.component.scss'],
})
export class EditGroupPermissionsComponent implements OnInit {
  public allPermissions: Permission[] = [];
  public selectedIds = new Set<number>();
  public loading = true;
  public filterText = '';

  constructor(
    public dialogRef: MatDialogRef<EditGroupPermissionsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditGroupPermissionsData,
    private groupsSvc: ManageGroupService,
    private permissionSvc: PermissionService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.permissionSvc.getPermissions().subscribe({
      next: (p) => {
        // assign asynchronously to avoid changing bindings during CD
        Promise.resolve().then(() => {
          this.allPermissions = p || [];
        });

        // once we have all permissions, fetch group-assigned permissions
        this.groupsSvc.getPermissionsForGroup(this.data.groupId).subscribe({
          next: (gp) => {
            Promise.resolve().then(() => {
              (gp || []).forEach((perm) => this.selectedIds.add(perm.permissionId));
              this.loading = false;
              this.cdr.detectChanges();
            });
          },
          error: () => {
            Promise.resolve().then(() => {
              this.selectedIds = new Set<number>();
              this.loading = false;
              this.cdr.detectChanges();
            });
          },
        });
      },
      error: () => {
        Promise.resolve().then(() => {
          this.allPermissions = [];
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  get filteredPermissions(): Permission[] {
    const f = this.filterText?.trim().toLowerCase();
    if (!f) return this.allPermissions;
    return this.allPermissions.filter((p) => (p.permissionName || '').toLowerCase().includes(f));
  }

  isSelected(id: number) {
    return this.selectedIds.has(id);
  }

  toggle(id: number) {
    if (this.selectedIds.has(id)) this.selectedIds.delete(id);
    else this.selectedIds.add(id);
  }

  save() {
    const dto = { permissionIds: Array.from(this.selectedIds) };
    this.groupsSvc.updateGroupPermissions(this.data.groupId, dto).subscribe({
      next: (result) => {
        // close and return the bulk-assignment result to the caller
        this.dialogRef.close(result);
      },
      error: (err) => {
        console.error('Update group permissions failed', err);
        this.dialogRef.close(null);
      },
    });
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
