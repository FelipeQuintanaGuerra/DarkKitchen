import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImporterComponent } from './importer/importer.component';

const routes: Routes = [
  { path: '', component: ImporterComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ImportersRoutingModule { }
