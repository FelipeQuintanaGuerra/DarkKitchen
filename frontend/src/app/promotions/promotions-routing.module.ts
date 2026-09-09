import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PromotionListComponent } from './promotion-list/promotion-list.component';
import { PromotionEditComponent } from './promotion-edit/promotion-edit.component';
import { PromotionCreateComponent } from './promotion-create/promotion-create.component';

const routes: Routes = [
  { path: '', component: PromotionListComponent },
  { path: 'create', component: PromotionCreateComponent },
  { path: 'edit/:id', component: PromotionEditComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PromotionsRoutingModule { }