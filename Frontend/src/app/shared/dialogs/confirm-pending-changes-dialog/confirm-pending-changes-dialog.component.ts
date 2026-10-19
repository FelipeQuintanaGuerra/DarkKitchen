import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirm-pending-changes-dialog',
  standalone: false,
  templateUrl: './confirm-pending-changes-dialog.component.html',
  styleUrls: ['./confirm-pending-changes-dialog.component.css']
})
export class ConfirmPendingChangesDialogComponent {
  constructor(
    private dialogRef: MatDialogRef<ConfirmPendingChangesDialogComponent>
  ) {}

  cancel(): void {
    this.dialogRef.close('cancel');
  }

  leave(): void {
    this.dialogRef.close('leave');
  }
}