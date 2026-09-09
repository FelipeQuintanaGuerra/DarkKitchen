import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ImporterService {
  private apiUrl = `${environment.apiBaseUrl}/api/importers`;

  constructor(private http: HttpClient) {}

  getImporterNames(): Observable<string[]> {
    return this.http.get<string[]>(this.apiUrl);
  }

  importProducts(importerName: string, filePath: string): Observable<{ imported: number }> {
    const params = new HttpParams()
      .set('importerName', importerName)
      .set('filePath', filePath);
    return this.http.post<{ imported: number }>(`${this.apiUrl}/import`, null, { params });
  }
}
