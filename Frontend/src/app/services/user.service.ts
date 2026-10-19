import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterUserRequest, UpdateUserRequest, UserSummary } from '../models/user.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiBaseUrl}/api/users`;

  constructor(private http: HttpClient) {}

  create(request: UpdateUserRequest): Observable<UserSummary> {
    return this.http.post<UserSummary>(this.apiUrl, request);
  }

  register(request: RegisterUserRequest): Observable<UserSummary> {
    return this.http.post<UserSummary>(`${this.apiUrl}/registrations`, request);
  }

  searchBy(name: string, surname: string): Observable<UserSummary[]> {
    const params = new HttpParams()
      .set('name', name)
      .set('surname', surname);
    return this.http.get<UserSummary[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<UserSummary> {
    return this.http.get<UserSummary>(`${this.apiUrl}/${id}`);
  }

  update(id: string, request: UpdateUserRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
