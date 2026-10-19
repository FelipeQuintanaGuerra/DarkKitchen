import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PromotionsRoutingModule } from './promotions-routing.module';
import { PromotionListComponent } from './promotion-list/promotion-list.component';
import { PromotionEditComponent } from './promotion-edit/promotion-edit.component';
import { PromotionCreateComponent } from './promotion-create/promotion-create.component';

@NgModule({
  declarations: [PromotionListComponent, PromotionEditComponent, PromotionCreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PromotionsRoutingModule
  ]
})
export class PromotionsModule { }