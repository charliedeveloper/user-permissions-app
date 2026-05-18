import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageGroupService } from './manage-group.service';
import { PermissionService } from '../../permission.service';

@Component({
  selector: 'edit-group-permissions-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-group-permissions-panel.component.html',
  styleUrls: ['./edit-group-permissions-panel.component.scss'],
})
export class EditGroupPermissionsPanelComponent implements OnChanges {
  @Input() group: any | null = null;
  @Output() close = new EventEmitter<boolean>();

  loading = false;
  isLoading = false;
  userName = '';
  allPermissions: any[] = [];
  assigned = new Set<number>();
  permissionFilterText = '';
  successMessage = '';
  errorMessage = '';
  assignedOnly = false;

  constructor(
    private groupsSvc: ManageGroupService,
    private permissionSvc: PermissionService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['group'] && this.group) {
      this.userName = this.group.groupName || '';
      this.loadData();
      // prevent body from scrolling while panel open
      document.body.style.overflow = 'hidden';
    }
  }

  private loadData() {
    this.loading = true;
    this.permissionSvc.getPermissions().subscribe({
      next: (all) => {
        Promise.resolve().then(() => {
          this.allPermissions = all || [];
          this.cdr.detectChanges();
          this.groupsSvc.getPermissionsForGroup(this.group.groupId).subscribe({
            next: (assigned) => {
              Promise.resolve().then(() => {
                this.assigned = new Set<number>((assigned || []).map((p: any) => p.permissionId));
                this.loading = false;
                this.cdr.detectChanges();
              });
            },
            error: (err) => {
              console.error(err);
              Promise.resolve().then(() => {
                this.assigned = new Set<number>();
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

  isPermissionAssigned(id: number) {
    return this.assigned.has(id);
  }

  togglePermission(id: number) {
    if (this.assigned.has(id)) this.assigned.delete(id);
    else this.assigned.add(id);
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

  saveRoles() {
    this.isLoading = true;
    const permissionIds = Array.from(this.assigned);
    this.groupsSvc.updateGroupPermissions(this.group.groupId, { permissionIds }).subscribe({
      next: (result) => {
        // show a concise success message using the returned counts
        this.successMessage = `Added ${result.permissionsAdded}, removed ${result.permissionsRemoved}, unchanged ${result.permissionsUnchanged}`;
        if (result.invalidPermissionIds?.length) {
          this.errorMessage = `Invalid IDs: ${result.invalidPermissionIds.join(', ')}`;
        }
        this.isLoading = false;
        // emit true so parent can refresh the group list
        this.close.emit(true);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Save failed';
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
