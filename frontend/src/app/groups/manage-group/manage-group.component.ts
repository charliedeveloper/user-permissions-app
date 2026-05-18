import { AfterViewInit, ChangeDetectorRef, Component, effect, inject, OnInit } from '@angular/core';
import { PermissionService } from '../../permission.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EditPermissionComponent } from '../../edit-permission.component';
import { EditGroupComponent } from './edit-group.component';
import { ConfirmDialogComponent } from '../../confirm-dialog.component';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { GroupWithPermissionCountDto, ManageGroupService } from './manage-group.service';
import { EditGroupPermissionsPanelComponent } from './edit-group-permissions-panel.component';

@Component({
  selector: 'app-manage-group',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    EditGroupPermissionsPanelComponent,
  ],
  templateUrl: './manage-group.component.html',
  styleUrls: ['./manage-group.component.scss'],
})
export class ManageGroupComponent implements OnInit, AfterViewInit {
  groupsSvc = inject(ManageGroupService);
  groups: GroupWithPermissionCountDto[] = [];
  displayedColumns = ['groupId', 'groupName', 'permissionCount', 'actions'];
  selectedGroupForPermissions: GroupWithPermissionCountDto | null = null;

  constructor(
    private svc: PermissionService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
  ) {
    effect(() => {
      const sig = this.groupsSvc.groupsSignal();
      if (sig !== undefined) {
        Promise.resolve().then(() => {
          this.groups = sig || [];
          this.cdr.detectChanges();
        });
      }
    });
  }

  managePermissions(p: GroupWithPermissionCountDto) {
    // show inline overlay panel component
    this.selectedGroupForPermissions = p;
  }

  onPermissionsClosed(reloaded: boolean) {
    this.selectedGroupForPermissions = null;
    if (reloaded) {
      this.groupsSvc.getGroupsWithPermissionCounts().subscribe({
        next: (g) => this.groupsSvc.groupsSignal.set(g || []),
        error: () => this.groupsSvc.groupsSignal.set([]),
      });
    }
  }

  ngAfterViewInit(): void {
    const sig = this.groupsSvc.groupsSignal();
    if (sig !== undefined) {
      this.groups = sig || [];
      this.cdr.detectChanges();
    }
  }

  ngOnInit(): void {
    this.groupsSvc.getGroupsWithPermissionCounts().subscribe({
      next: (g) => {
        this.groupsSvc.groupsSignal.set(g || []);
      },
      error: () => {
        this.groupsSvc.groupsSignal.set([]);
      },
    });
  }

  edit(p: GroupWithPermissionCountDto) {
    const ref = this.dialog.open(EditGroupComponent, {
      data: {
        groupId: p.groupId,
        groupName: p.groupName,
        isActive: p.isActive ?? true,
      },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      // result contains GroupName
      this.groupsSvc.updateGroup(p.groupId, { GroupName: result.GroupName }).subscribe({
        next: () =>
          this.groupsSvc.getGroupsWithPermissionCounts().subscribe({
            next: (g) => this.groupsSvc.groupsSignal.set(g || []),
            error: () => this.groupsSvc.groupsSignal.set([]),
          }),
        error: (err) => console.error('Update failed', err),
      });
    });
  }

  deactivate(p: GroupWithPermissionCountDto) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Permission',
        message: `Are you sure you want to delete permission "${p.groupName}"? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.svc.deletePermission(p.groupId).subscribe({
        next: () =>
          this.groupsSvc.getGroupsWithPermissionCounts().subscribe({
            next: (g) => this.groupsSvc.groupsSignal.set(g || []),
            error: () => this.groupsSvc.groupsSignal.set([]),
          }),
        error: (err) => {
          console.error('Delete failed', err);
          // fallback: mark inactive locally
          p.isActive = false;
        },
      });
    });
  }

  onAddGroup() {
    const ref = this.dialog.open(EditGroupComponent, {
      data: {
        groupId: 0,
        groupName: '',
        isActive: true,
      },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.groupsSvc
        .createGroup({
          GroupName: result.GroupName,
          IsActive: result.IsActive,
        })
        .subscribe({
          next: () =>
            this.groupsSvc.getGroupsWithPermissionCounts().subscribe({
              next: (g) => this.groupsSvc.groupsSignal.set(g || []),
              error: () => this.groupsSvc.groupsSignal.set([]),
            }),
          error: (err) => console.error('Create failed', err),
        });
    });
  }
}
