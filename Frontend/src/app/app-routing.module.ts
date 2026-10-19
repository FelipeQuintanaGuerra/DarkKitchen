import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: 'home',
    component: HomeComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'users',
    loadChildren: () =>
      import('./users/users.module').then((m) => m.UsersModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'products',
    loadChildren: () =>
      import('./products/products.module').then((m) => m.ProductsModule),
    canActivate: [AuthGuard],
  },
  {
    path: 'promotions',
    loadChildren: () =>
      import('./promotions/promotions.module').then((m) => m.PromotionsModule),
    canActivate: [AuthGuard],
  },
  {
    path: 'orders',
    loadChildren: () =>
      import('./orders/orders.module').then((m) => m.OrdersModule),
    canActivate: [AuthGuard],
  },
  {
    path: 'shipping-types',
    loadChildren: () =>
      import('./shipping-types/shipping-types.module').then(
        (m) => m.ShippingTypesModule,
      ),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'reports',
    loadChildren: () =>
      import('./reports/reports.module').then((m) => m.ReportsModule),
    canActivate: [AuthGuard],
  },
  {
    path: 'audits',
    loadChildren: () =>
      import('./audits/audits.module').then((m) => m.AuditsModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'importers',
    loadChildren: () =>
      import('./importers/importers.module').then((m) => m.ImportersModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: '',
    loadChildren: () =>
      import('./authentication/authentication.module').then(
        (m) => m.AuthenticationModule,
      ),
  },
  {
    path: '**',
    redirectTo: 'home',
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
