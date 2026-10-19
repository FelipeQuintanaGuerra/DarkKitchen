import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { OrderCreateComponent } from './order-create/order-create.component';
import { MyOrdersComponent } from './my-orders/my-orders.component';
import { FilteredOrdersComponent } from './filtered-orders/filtered-orders.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';

import { AuthGuard } from '../guards/auth.guard';
import { RoleGuard } from '../guards/role.guard';
import { PendingChangesGuard } from '../guards/pending-changes.guard';

const routes: Routes = [
  {
    path: 'create',
    component: OrderCreateComponent,
    canActivate: [AuthGuard, RoleGuard],
    canDeactivate: [PendingChangesGuard],
    data: { roles: ['Client'] },
  },
  {
    path: 'my-orders',
    component: MyOrdersComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Client'] },
  },
  {
    path: 'filtered',
    component: FilteredOrdersComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Dispatcher', 'Admin'] },
  },
  {
    path: 'detail',
    component: OrderDetailComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Dispatcher', 'Admin'] },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OrdersRoutingModule {}
