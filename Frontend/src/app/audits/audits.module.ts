import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AuditsRoutingModule } from './audits-routing.module';
import { AuditSearchComponent } from './audit-search/audit-search.component';

@NgModule({
  declarations: [
    AuditSearchComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    AuditsRoutingModule
  ]
})
export class AuditsModule { }