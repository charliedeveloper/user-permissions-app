import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

export interface EditGroupData {
  groupId: number;
  groupName: string;
  isActive?: boolean;
}

@Component({
  selector: 'edit-group-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
  ],
  templateUrl: './edit-group.component.html',
  styleUrls: ['./edit-group.component.scss'],
})
export class EditGroupComponent {
  public model: EditGroupData;

  constructor(
    public dialogRef: MatDialogRef<EditGroupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditGroupData,
  ) {
    this.model = { ...data };
  }

  save() {
    const dto = {
      GroupName: this.model.groupName,
    };
    this.dialogRef.close(dto);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
