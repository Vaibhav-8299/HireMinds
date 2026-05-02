import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Test, TestResponse, CreateTestRequest, AddQuestionRequest } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private readonly apiUrl = 'http://localhost:5034/api/tests';
  private http = inject(HttpClient);

  getMyTests(): Observable<Test[]> {
    return this.http.get<Test[]>(`${this.apiUrl}/my`);
  }

  getTestById(id: number): Observable<TestResponse> {
    return this.http.get<TestResponse>(`${this.apiUrl}/${id}`);
  }

  createTest(dto: CreateTestRequest): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  addQuestion(testId: number, dto: AddQuestionRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${testId}/questions`, dto);
  }

  updateTest(id: number, dto: { title: string; timeLimitMinutes: number }): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, dto);
  }

  updateQuestion(testId: number, questionId: number, dto: AddQuestionRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${testId}/questions/${questionId}`, dto);
  }

  deleteQuestion(testId: number, questionId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${testId}/questions/${questionId}`);
  }
}
