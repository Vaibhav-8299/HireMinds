import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubmitTestRequest, TestResult } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class SubmissionService {
  private readonly apiUrl = 'https://hireminds1.runasp.net/api';
  private http = inject(HttpClient);

  submitTest(applicationId: number, dto: SubmitTestRequest): Observable<TestResult> {
    return this.http.post<TestResult>(`${this.apiUrl}/submit/${applicationId}`, dto);
  }

  getResult(applicationId: number): Observable<TestResult> {
    return this.http.get<TestResult>(`${this.apiUrl}/result/${applicationId}`);
  }
}
