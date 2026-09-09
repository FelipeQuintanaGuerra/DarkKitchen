import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ShippingTypesRoutingModule } from './shipping-types-routing.module';
import { ShippingTypeListComponent } from './shipping-type-list/shipping-type-list.component';
import { ShippingTypeCreateComponent } from './shipping-type-create/shipping-type-create.component';
import { ShippingTypeEditComponent } from './shipping-type-edit/shipping-type-edit.component';

@NgModule({
  declarations: [ShippingTypeListComponent, ShippingTypeCreateComponent, ShippingTypeEditComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ShippingTypesRoutingModule
  ]
})
export class ShippingTypesModule { }
