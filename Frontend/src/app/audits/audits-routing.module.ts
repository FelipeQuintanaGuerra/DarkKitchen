import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuditSearchComponent } from './audit-search/audit-search.component';

import { AuthGuard } from '../guards/auth.guard';
import { RoleGuard } from '../guards/role.guard';

const routes: Routes = [
  {
    path: '',
    component: AuditSearchComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuditsRoutingModule { }