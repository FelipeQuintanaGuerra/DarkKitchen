import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchAuditRequest, AuditLogResponse } from '../models/audit.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private apiUrl = `${environment.apiBaseUrl}/api/audit`;

  constructor(private http: HttpClient) {}

  search(request: SearchAuditRequest): Observable<AuditLogResponse[]> {
  let params = new HttpParams()
    .set('from', request.dateFrom)
    .set('to', request.dateTo)
    .set('entityName', request.entityName);

  if (request.entityId) {
    params = params.set('entityId', request.entityId);
  }

  return this.http.get<AuditLogResponse[]>(this.apiUrl, { params });
}
}
