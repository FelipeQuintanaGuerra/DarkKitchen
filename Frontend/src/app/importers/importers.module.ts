import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImportersRoutingModule } from './importers-routing.module';
import { ImporterComponent } from './importer/importer.component';

@NgModule({
  declarations: [
    ImporterComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ImportersRoutingModule
  ]
})
export class ImportersModule { }
