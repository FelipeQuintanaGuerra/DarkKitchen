import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TopProductsReportComponent } from './top-products-report/top-products-report.component';
import { SalesReportComponent } from './sales-report/sales-report.component';

const routes: Routes = [
  { path: 'top-products', component: TopProductsReportComponent },
  { path: 'sales', component: SalesReportComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportsRoutingModule { }
