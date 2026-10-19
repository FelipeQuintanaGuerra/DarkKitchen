import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrderCreateComponent } from './order-create/order-create.component';
import { MyOrdersComponent } from './my-orders/my-orders.component';
import { FilteredOrdersComponent } from './filtered-orders/filtered-orders.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    OrderCreateComponent,
    MyOrdersComponent,
    FilteredOrdersComponent,
    OrderDetailComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    OrdersRoutingModule,
    SharedModule
  ]
})
export class OrdersModule { }
