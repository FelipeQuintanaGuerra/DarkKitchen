import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { map, Observable } from 'rxjs';
import { HasUnsavedChanges } from './has-unsaved-changes';
import { ConfirmPendingChangesDialogComponent } from '../shared/dialogs/confirm-pending-changes-dialog/confirm-pending-changes-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class PendingChangesGuard implements CanDeactivate<HasUnsavedChanges> {
  constructor(private dialog: MatDialog) {}

  canDeactivate(component: HasUnsavedChanges): boolean | Observable<boolean> {
    const hasChanges = component.hasUnsavedChanges();

    if (!hasChanges) {
      return true;
    }

    const dialogRef = this.dialog.open(ConfirmPendingChangesDialogComponent, {
      disableClose: true,
      width: '420px'
    });

    return dialogRef.afterClosed().pipe(
      map(result => result === 'leave')
    );
  }
}