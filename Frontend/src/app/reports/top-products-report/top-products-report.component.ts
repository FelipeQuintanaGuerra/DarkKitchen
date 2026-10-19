import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ReportService } from '../../services/report.service';
import { TopProductDto } from '../../models/report.model';

@Component({
  selector: 'app-top-products-report',
  standalone: false,
  templateUrl: './top-products-report.component.html',
  styleUrl: './top-products-report.component.css',
})
export class TopProductsReportComponent implements OnDestroy {
  from = '';
  to = '';

  products: TopProductDto[] = [];
  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;

  constructor(private reportService: ReportService) {}

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  search(): void {
    this.errorMessage = null;
    this.products = [];

    if (!this.from || !this.to) {
      this.errorMessage = 'Las fechas desde y hasta son obligatorias';
      return;
    }

    if (this.from > this.to) {
      this.errorMessage =
        'La fecha desde no puede ser mayor que la fecha hasta';
      return;
    }

    this.searchSubscription = this.reportService
      .getTopProducts(`${this.from}T00:00:00`, `${this.to}T23:59:59`)
      .subscribe({
        next: (products) => {
          this.products = products;

          if (products.length === 0) {
            this.errorMessage =
              'No hay productos vendidos en el rango seleccionado';
          }
        },
        error: (err) => {
          const error = err.error;
          this.errorMessage =
            typeof error === 'string'
              ? error
              : error?.message || 'Error al obtener el reporte';
        },
      });
  }
}
