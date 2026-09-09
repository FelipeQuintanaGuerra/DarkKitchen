import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { map, Observable } from 'rxjs';
import { ConfirmLogoutDialogComponent } from '../authentication/confirm-logout-dialog/confirm-logout-dialog.component';
@Injectable({
  providedIn: 'root',
})
export class GuestGuard implements CanActivate {
  constructor(
    private router: Router,
    private dialog: MatDialog,
  ) {}

  canActivate(): boolean | Observable<boolean> {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');

    if (!token || !user) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      return true;
    }

    const dialogRef = this.dialog.open(ConfirmLogoutDialogComponent, {
      disableClose: true,
      width: '420px',
    });

    return dialogRef.afterClosed().pipe(
      map((result) => {
        if (result === 'logout') {
          localStorage.clear();
          return true;
        }

        this.router.navigate(['/home']);
        return false;
      }),
    );
  }
}
