import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { SaveUserDto } from './user.service';
import { validAlphaText } from './input-validator';

export interface EditUserData {
  userId?: number;
  firstName?: string;
  lastName?: string;
}

@Component({
  selector: 'edit-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './edit-user.component.html',
  styleUrls: ['./edit-user.component.scss'],
})
export class EditUserComponent {
  readonly data: EditUserData;
  readonly model;

  constructor(
    private readonly fb: FormBuilder,
    public readonly dialogRef: MatDialogRef<EditUserComponent, SaveUserDto | null>,
    @Inject(MAT_DIALOG_DATA) data: EditUserData,
  ) {
    this.data = data;

    this.model = this.fb.nonNullable.group({
      firstName: [
        data.firstName ?? '',
        [Validators.required, Validators.maxLength(5), (c: AbstractControl) => validAlphaText(c)],
      ],
      lastName: [
        data.lastName ?? '',
        [Validators.required, Validators.maxLength(5), (c: AbstractControl) => validAlphaText(c)],
      ],
    });
  }

  get isEditMode(): boolean {
    return !!this.data.userId;
  }

  isNameInvalid(controlName: 'firstName' | 'lastName'): boolean {
    const control = this.model.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  getNameErrorMessage(controlName: 'firstName' | 'lastName'): string {
    const control = this.model.controls[controlName];
    if (control.hasError('required')) {
      return 'This field is required';
    }
    return 'Invalid name';
  }

  save(): void {
    if (this.model.invalid) {
      this.model.markAllAsTouched();
      return;
    }

    const dto: SaveUserDto = {
      firstName: this.model.controls.firstName.value.trim(),
      lastName: this.model.controls.lastName.value.trim(),
    };

    this.dialogRef.close(dto);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
