import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Job, CreateJobRequest } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = 'http://localhost:5034/api/jobs';
  private http = inject(HttpClient);

  getJobs(search?: string, location?: string, company?: string): Observable<Job[]> {
    let params = new HttpParams();
    
    if (search) params = params.set('search', search);
    if (location) params = params.set('location', location);
    if (company) params = params.set('company', company);

    return this.http.get<Job[]>(this.apiUrl, { params });
  }

  getMyJobs(): Observable<Job[]> {
    return this.http.get<Job[]>(`${this.apiUrl}/my`);
  }

  getJobById(id: number): Observable<Job> {
    return this.http.get<Job>(`${this.apiUrl}/${id}`);
  }

  createJob(dto: CreateJobRequest): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }
}
