import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProfileDto, ProfileResponse } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  // Using 5034 to match the actual running backend port from earlier steps
  private readonly apiUrl = 'https://hireminds1.runasp.net/api/profile';
  private http = inject(HttpClient);

  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(this.apiUrl);
  }

  updateProfile(dto: ProfileDto): Observable<any> {
    return this.http.put(this.apiUrl, dto);
  }

  uploadResume(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<{ url: string }>(`${this.apiUrl}/resume`, formData);
  }
}
