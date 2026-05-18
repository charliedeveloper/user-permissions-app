import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

export interface EditPermissionData {
  permissionId: number;
  permissionKey: string;
  permissionName: string;
  isActive?: boolean;
}

@Component({
  selector: 'edit-permission-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
  ],
  templateUrl: './edit-permission.component.html',
  styleUrls: ['./edit-permission.component.scss'],
})
export class EditPermissionComponent {
  public model: EditPermissionData;

  constructor(
    public dialogRef: MatDialogRef<EditPermissionComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditPermissionData,
  ) {
    this.model = { ...data };
  }

  save() {
    const dto = {
      PermissionKey: this.model.permissionKey,
      PermissionName: this.model.permissionName,
      IsActive: this.model.isActive ?? true,
    };
    this.dialogRef.close(dto);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
