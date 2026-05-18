import {
  Component,
  effect,
  EventEmitter,
  OnInit,
  Output,
  AfterViewInit,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDialog } from '@angular/material/dialog';
import { EditUserComponent } from './edit-user.component';
import { SaveUserDto, User, UserService } from './user.service';

@Component({
  selector: 'user-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatToolbarModule,
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss'],
})
export class UserListComponent implements OnInit, AfterViewInit {
  users: User[] = [];
  selectedUser: User | null = null;
  selectedUserName: string | null = null;
  filter = '';

  @Output() selectUser = new EventEmitter<User>();

  constructor(
    private userService: UserService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
  ) {
    effect(() => {
      const sig = this.userService.usersSignal();
      if (sig !== undefined) {
        Promise.resolve().then(() => {
          this.users = sig || [];
          this.cdr.detectChanges();
        });
      }
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  ngAfterViewInit(): void {
    const sig = this.userService.usersSignal();
    if (sig !== undefined) {
      this.users = sig || [];
      this.cdr.detectChanges();
    }
  }

  onSelect(user: User) {
    this.selectedUser = user;
    this.selectedUserName = user.userName;
    this.selectUser.emit(user);
  }

  onAddUser() {
    const ref = this.dialog.open(EditUserComponent, {
      data: {
        firstName: '',
        lastName: '',
      },
    });

    ref.afterClosed().subscribe((dto: SaveUserDto | null | undefined) => {
      if (!dto) {
        return;
      }

      this.userService.createUser(dto).subscribe({
        next: () => this.loadUsers(),
        error: (err) => console.error('Create user failed', err),
      });
    });
  }

  onEditUser() {
    if (!this.selectedUser) {
      return;
    }

    const ref = this.dialog.open(EditUserComponent, {
      data: {
        userId: this.selectedUser.userId,
        firstName: this.selectedUser.firstName ?? '',
        lastName: this.selectedUser.lastName ?? '',
      },
    });

    ref.afterClosed().subscribe((dto: SaveUserDto | null | undefined) => {
      if (!dto) {
        return;
      }

      this.userService.updateUser(this.selectedUser!.userId, dto).subscribe({
        next: () => this.loadUsers(),
        error: (err) => console.error('Update user failed', err),
      });
    });
  }

  private loadUsers() {
    this.userService.getUsers().subscribe({
      next: (u) => {
        this.userService.usersSignal.set(u);
      },
      error: () => {
        this.userService.usersSignal.set([]);
      },
    });
  }
}
