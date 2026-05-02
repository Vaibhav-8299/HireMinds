import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Application, ApplicationResponse } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {
  private readonly apiUrl = 'https://hireminds1.runasp.net/api/applications';
  private http = inject(HttpClient);

  applyToJob(jobId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${jobId}/apply`, {});
  }

  getMyApplications(): Observable<ApplicationResponse[]> {
    return this.http.get<ApplicationResponse[]>(`${this.apiUrl}/my`);
  }

  getApplicationsByJob(jobId: number): Observable<ApplicationResponse[]> {
    return this.http.get<ApplicationResponse[]>(`${this.apiUrl}/job/${jobId}`);
  }

  getApplicationById(id: number): Observable<ApplicationResponse> {
    return this.http.get<ApplicationResponse>(`${this.apiUrl}/${id}`);
  }

  updateStatus(id: number, status: string, message?: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/status`, { status, message });
  }

  getNotifications(): Observable<ApplicationResponse[]> {
    return this.http.get<ApplicationResponse[]>(`${this.apiUrl}/notifications`);
  }
}
