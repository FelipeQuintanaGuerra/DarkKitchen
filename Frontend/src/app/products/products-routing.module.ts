import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductListComponent } from './product-list/product-list.component';
import { ProductCreateComponent } from './product-create/product-create.component';
import { ProductEditComponent } from './product-edit/product-edit.component';
import { AuthGuard } from '../guards/auth.guard';
import { RoleGuard } from '../guards/role.guard';
import { PendingChangesGuard } from '../guards/pending-changes.guard';

const routes: Routes = [
  {
    path: '',
    component: ProductListComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Client'] }
  },
  {
    path: 'create',
    component: ProductCreateComponent,
    canActivate: [AuthGuard, RoleGuard],
    canDeactivate: [PendingChangesGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'edit/:id',
    component: ProductEditComponent,
    canActivate: [AuthGuard, RoleGuard],
    canDeactivate: [PendingChangesGuard],
    data: { roles: ['Admin'] }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProductsRoutingModule { }