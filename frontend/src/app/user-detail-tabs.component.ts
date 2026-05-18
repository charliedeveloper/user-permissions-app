import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { PermissionComponent } from './permission.component';
import { ManageGroupComponent } from './groups/manage-group/manage-group.component';
import { User } from './users/user.service';
import { UserPermissionsComponent } from './users/user-permissions.component';

@Component({
  selector: 'user-detail-tabs',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTabsModule,
    MatButtonModule,
    PermissionComponent,
    ManageGroupComponent,
    UserPermissionsComponent,
  ],
  templateUrl: './user-detail-tabs.component.html',
  styleUrls: ['./user-detail-tabs.component.scss'],
})
export class UserDetailTabsComponent implements OnChanges {
  @Input() user: User | null = null;
  activeIndex = 0;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['user'] && !changes['user'].firstChange) {
      this.activeIndex = 0;
    }
  }
}
