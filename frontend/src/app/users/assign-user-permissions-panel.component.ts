import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionDto, User, UserService } from './user.service';
import { PermissionService } from '../permission.service';
import { ManageGroupService } from '../groups/manage-group/manage-group.service';

@Component({
  selector: 'assign-user-permissions-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assign-user-permissions-panel.component.html',
  styleUrls: ['./assign-user-permissions-panel.component.scss'],
})
export class AssignUserPermissionsPanelComponent implements OnChanges, OnDestroy {
  @Input() user: User | null = null;
  @Input() existingPermissions: PermissionDto[] = [];
  @Output() close = new EventEmitter<boolean>();

  loading = false;
  isLoading = false;
  userName = '';
  allPermissions: any[] = [];
  assigned = new Set<number>();
  disabledPermissionKeys = new Set<string>();
  permissionFilterText = '';
  successMessage = '';
  errorMessage = '';
  assignedOnly = false;
  disabledPermissionSources = new Map<string, string>();

  allGroups: any[] = [];
  assignedGroups = new Set<number>();
  groupFilterText = '';
  assignedGroupsOnly = false;

  constructor(
    private userSvc: UserService,
    private permissionSvc: PermissionService,
    private groupSvc: ManageGroupService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['user'] && this.user) {
      this.userName = this.user.userName || '';
      this.loadData();
      // prevent body from scrolling while panel open
      document.body.style.overflow = 'hidden';
    }
    if (changes['existingPermissions'] && this.allPermissions.length > 0) {
      this.updateAssignedFromExisting();
    }
  }

  private loadData() {
    this.loading = true;
    this.permissionSvc.getPermissions().subscribe({
      next: (all) => {
        Promise.resolve().then(() => {
          this.allPermissions = all || [];
          this.updateAssignedFromExisting();
          this.cdr.detectChanges();

          // Load groups
          this.groupSvc.getGroupsWithPermissionCounts().subscribe({
            next: (groups) => {
              Promise.resolve().then(() => {
                this.allGroups = groups || [];
                this.cdr.detectChanges();

                // Load user's assigned groups
                this.userSvc.getGroupsForUser(this.user!.userId).subscribe({
                  next: (userGroups) => {
                    Promise.resolve().then(() => {
                      this.assignedGroups = new Set<number>(
                        (userGroups || []).map((g: any) => g.groupId),
                      );
                      this.loading = false;
                      this.cdr.detectChanges();
                    });
                  },
                  error: (err) => {
                    console.error(err);
                    Promise.resolve().then(() => {
                      this.assignedGroups = new Set<number>();
                      this.loading = false;
                      this.cdr.detectChanges();
                    });
                  },
                });
              });
            },
            error: (err) => {
              console.error(err);
              Promise.resolve().then(() => {
                this.allGroups = [];
                this.loading = false;
                this.cdr.detectChanges();
              });
            },
          });
        });
      },
      error: (err) => {
        console.error(err);
        Promise.resolve().then(() => {
          this.allPermissions = [];
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  private updateAssignedFromExisting() {
    this.assigned = new Set<number>();
    this.disabledPermissionKeys = new Set<string>();
    if (!this.existingPermissions || this.existingPermissions.length === 0) {
      return;
    }
    this.disabledPermissionSources = new Map<string, string>();

    const existingKeys = new Set(this.existingPermissions.map((p) => p.permissionKey));
    this.disabledPermissionKeys = new Set(
      this.existingPermissions.filter((p) => p.groupName != null).map((p) => p.permissionKey),
    );

    for (const permission of this.allPermissions) {
      if (existingKeys.has(permission.permissionKey)) {
        this.assigned.add(permission.permissionId);
      }
    }
    for (const permission of this.existingPermissions) {
      if (permission.groupName != null) {
        this.disabledPermissionSources.set(permission.permissionKey, permission.groupName);
      }
    }
  }

  isPermissionAssigned(id: number) {
    return this.assigned.has(id);
  }

  isPermissionDisabled(permissionKey: string) {
    return this.disabledPermissionKeys.has(permissionKey);
  }

  getPermissionSource(permissionKey: string) {
    return this.disabledPermissionSources.get(permissionKey) || null;
  }

  togglePermission(id: number) {
    if (this.assigned.has(id)) this.assigned.delete(id);
    else this.assigned.add(id);
  }

  isGroupAssigned(id: number) {
    return this.assignedGroups.has(id);
  }

  toggleGroup(id: number) {
    if (this.assignedGroups.has(id)) this.assignedGroups.delete(id);
    else this.assignedGroups.add(id);
  }

  getFilteredPermissions() {
    const q = (this.permissionFilterText || '').toLowerCase();
    let list = this.allPermissions;
    if (this.assignedOnly) {
      list = list.filter((p) => this.assigned.has(p.permissionId));
    }
    if (!q) return list;
    return list.filter((p) => (p.permissionName || '').toLowerCase().includes(q));
  }

  getFilteredGroups() {
    const q = (this.groupFilterText || '').toLowerCase();
    let list = this.allGroups;
    if (this.assignedGroupsOnly) {
      list = list.filter((g) => this.assignedGroups.has(g.groupId));
    }
    if (!q) return list;
    return list.filter((g) => (g.groupName || '').toLowerCase().includes(q));
  }

  saveRoles() {
    this.isLoading = true;
    const permissionKeyById = new Map<number, string>(
      this.allPermissions.map((p) => [p.permissionId, p.permissionKey]),
    );
    const permissionIds = Array.from(this.assigned).filter((id) => {
      const key = permissionKeyById.get(id);
      return !key || !this.isPermissionDisabled(key);
    });
    const groupIds = Array.from(this.assignedGroups);

    this.userSvc.updateUserPermissions(this.user!.userId, { permissionIds, groupIds }).subscribe({
      next: (result) => {
        this.successMessage = `Permissions: Added ${result.permissionsAdded}, removed ${result.permissionsRemoved}. Groups: Added ${result.groupsAdded}, removed ${result.groupsRemoved}`;
        if (result.invalidPermissionIds?.length || result.invalidGroupIds?.length) {
          const errors = [];
          if (result.invalidPermissionIds?.length) {
            errors.push(`Invalid permission IDs: ${result.invalidPermissionIds.join(', ')}`);
          }
          if (result.invalidGroupIds?.length) {
            errors.push(`Invalid group IDs: ${result.invalidGroupIds.join(', ')}`);
          }
          this.errorMessage = errors.join('; ');
        }
        this.isLoading = false;
        // emit true so parent can refresh the user list
        this.close.emit(true);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to save roles';
        this.isLoading = false;
      },
    });
  }

  closePanel() {
    this.close.emit(false);
    document.body.style.overflow = '';
  }

  ngOnDestroy(): void {
    // restore body scroll if component is destroyed
    document.body.style.overflow = '';
  }
}
