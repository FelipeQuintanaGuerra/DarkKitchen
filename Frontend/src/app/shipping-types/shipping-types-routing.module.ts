import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ShippingTypeListComponent } from './shipping-type-list/shipping-type-list.component';
import { ShippingTypeCreateComponent } from './shipping-type-create/shipping-type-create.component';
import { ShippingTypeEditComponent } from './shipping-type-edit/shipping-type-edit.component';

const routes: Routes = [
  { path: '', component: ShippingTypeListComponent },
  { path: 'create', component: ShippingTypeCreateComponent },
  { path: 'edit/:id', component: ShippingTypeEditComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ShippingTypesRoutingModule { }
