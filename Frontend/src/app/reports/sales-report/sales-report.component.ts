import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ReportService } from '../../services/report.service';
import { MonthlySalesDto } from '../../models/report.model';

@Component({
  selector: 'app-sales-report',
  standalone: false,
  templateUrl: './sales-report.component.html',
  styleUrl: './sales-report.component.css'
})
export class SalesReportComponent implements OnDestroy {
  from = '';
  to = '';

  reports: MonthlySalesDto[] = [];
  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;

  constructor(private reportService: ReportService) {}

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  search(): void {
    this.errorMessage = null;
    this.reports = [];

    if (!this.from || !this.to) {
      this.errorMessage = 'Las fechas desde y hasta son obligatorias';
      return;
    }

    if (this.from > this.to) {
      this.errorMessage = 'La fecha desde no puede ser mayor que la fecha hasta';
      return;
    }

    this.searchSubscription = this.reportService
      .getSalesReport(
        `${this.from}T00:00:00`,
        `${this.to}T23:59:59`
      )
      .subscribe({
        next: reports => {
          this.reports = reports;

          if (reports.length === 0) {
            this.errorMessage =
              'No hay ventas en el rango seleccionado';
          }
        },
        error: err => {
          const error = err.error;

          this.errorMessage =
            typeof error === 'string'
              ? error
              : error?.message || 'Error al obtener reporte';
        }
      });
  }

  getMonthTotal(report: MonthlySalesDto): number {
    return report.customers.reduce(
      (sum, customer) => sum + customer.total,
      0
    );
  }

  getGrandTotal(): number {
    return this.reports.reduce(
      (sum, report) => sum + this.getMonthTotal(report),
      0
    );
  }
}