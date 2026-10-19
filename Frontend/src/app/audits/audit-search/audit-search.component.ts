import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuditService } from '../../services/audit.service';
import { AuditLogResponse, SearchAuditRequest } from '../../models/audit.model';

@Component({
  selector: 'app-audit-search',
  standalone: false,
  templateUrl: './audit-search.component.html',
  styleUrl: './audit-search.component.css',
})
export class AuditSearchComponent implements OnDestroy {
  request: SearchAuditRequest = {
    dateFrom: '',
    dateTo: '',
    entityName: 'PRODUCTO',
    entityId: '',
  };

  logs: AuditLogResponse[] = [];
  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;

  constructor(private auditService: AuditService) {}

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  search(): void {
    this.errorMessage = null;
    this.logs = [];

    if (!this.request.dateFrom || !this.request.dateTo) {
      this.errorMessage = 'Las fechas desde y hasta son obligatorias';
      return;
    }

    if (!this.request.entityName.trim()) {
      this.errorMessage = 'La entidad es obligatoria';
      return;
    }

    if (this.request.dateFrom > this.request.dateTo) {
      this.errorMessage =
        'La fecha desde no puede ser mayor que la fecha hasta';
      return;
    }

    const requestToSend: SearchAuditRequest = {
      ...this.request,
      dateFrom: `${this.request.dateFrom}:00`,
      dateTo: `${this.request.dateTo}:59`,
    };

    this.searchSubscription = this.auditService.search(requestToSend).subscribe({
      next: (logs) => {
        this.logs = logs;

        if (logs.length === 0) {
          this.errorMessage =
            'No hay registros de auditoría para los filtros indicados';
        }
      },
      error: (err) => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al consultar auditoría';
      },
    });
  }
}
