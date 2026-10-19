import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirm-logout-dialog',
  standalone: false,
  templateUrl: './confirm-logout-dialog.component.html',
  styleUrl: './confirm-logout-dialog.component.css'
})
export class ConfirmLogoutDialogComponent {
  constructor(
    private dialogRef: MatDialogRef<ConfirmLogoutDialogComponent>
  ) {}

  cancel(): void {
    this.dialogRef.close('cancel');
  }

  logout(): void {
    this.dialogRef.close('logout');
  }
}