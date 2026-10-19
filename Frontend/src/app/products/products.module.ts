import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProductsRoutingModule } from './products-routing.module';
import { ProductListComponent } from './product-list/product-list.component';
import { ProductCreateComponent } from './product-create/product-create.component';
import { ProductEditComponent } from './product-edit/product-edit.component';
import { AbsoluteUrlPipe } from '../pipes/absolute-url.pipe';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [ProductListComponent, ProductCreateComponent, ProductEditComponent, AbsoluteUrlPipe],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ProductsRoutingModule,
    SharedModule
  ]
})
export class ProductsModule { }
