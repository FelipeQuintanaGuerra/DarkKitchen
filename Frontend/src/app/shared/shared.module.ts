import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { ConfirmPendingChangesDialogComponent } from './dialogs/confirm-pending-changes-dialog/confirm-pending-changes-dialog.component';

@NgModule({
  declarations: [
    ConfirmPendingChangesDialogComponent
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule
  ],
  exports: [
    ConfirmPendingChangesDialogComponent,
    MatDialogModule,
    MatButtonModule
  ]
})
export class SharedModule { }