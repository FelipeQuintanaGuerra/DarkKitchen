import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsRoutingModule } from './reports-routing.module';
import { TopProductsReportComponent } from './top-products-report/top-products-report.component';
import { SalesReportComponent } from './sales-report/sales-report.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    TopProductsReportComponent,
    SalesReportComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReportsRoutingModule
  ]
})
export class ReportsModule { }
