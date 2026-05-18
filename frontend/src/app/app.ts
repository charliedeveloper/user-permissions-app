import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { UserListComponent } from './users/user-list.component';
import { UserDetailTabsComponent } from './user-detail-tabs.component';
import { User } from './users/user.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, HttpClientModule, UserListComponent, UserDetailTabsComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
})
export class App {
  protected readonly title = signal('permissions');

  protected selectedUser: User | null = null;

  protected onUserSelected(user: User) {
    this.selectedUser = user;
  }
}
