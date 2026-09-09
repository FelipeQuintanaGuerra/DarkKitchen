import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const userJson = localStorage.getItem('user');

    if (!userJson) {
      this.router.navigate(['/login']);
      return false;
    }

    const user = JSON.parse(userJson);
    const roleMap: { [key: number]: string } = { 0: 'Client', 1: 'Dispatcher', 2: 'Admin' };
    const userRole = typeof user.role === 'number' ? roleMap[user.role] : user.role;
    const allowedRoles = route.data['roles'] as string[];

    if (allowedRoles && allowedRoles.includes(userRole)) {
      return true;
    }

    this.router.navigate(['/']);
    return false;
  }
}
