import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TopProductDto, MonthlySalesDto } from '../models/report.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private apiUrl = `${environment.apiBaseUrl}/api/reports`;

  constructor(private http: HttpClient) {}

  getTopProducts(from: string, to: string): Observable<TopProductDto[]> {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to);
    return this.http.get<TopProductDto[]>(`${this.apiUrl}/top-products`, { params });
  }

  getSalesReport(from: string, to: string): Observable<MonthlySalesDto[]> {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to);
    return this.http.get<MonthlySalesDto[]>(`${this.apiUrl}/sales`, { params });
  }
}
