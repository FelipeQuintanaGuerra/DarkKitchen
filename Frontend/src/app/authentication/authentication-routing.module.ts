import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginFormComponent } from './login-form/login-form.component';
import { RegisterFormComponent } from './register-form/register-form.component';
import { GuestGuard } from '../guards/guest.guard';

const routes: Routes = [
  {
    path: 'login',
    component: LoginFormComponent,
    canActivate: [GuestGuard]
  },
  {
    path: 'register',
    component: RegisterFormComponent,
    canActivate: [GuestGuard]
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthenticationRoutingModule {}