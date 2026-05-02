import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationResponse } from '../../../core/models/interfaces';

@Component({
  selector: 'app-my-applications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.css'
})
export class MyApplicationsComponent implements OnInit {
  applications = signal<ApplicationResponse[]>([]);
  isLoading = signal<boolean>(true);

  private applicationService = inject(ApplicationService);

  ngOnInit() {
    this.applicationService.getMyApplications().subscribe({
      next: (data) => {
        // Sort by applied date descending
        const sorted = data.sort((a, b) => new Date(b.appliedAt).getTime() - new Date(a.appliedAt).getTime());
        this.applications.set(sorted);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  }

  // Helper method to determine CSS class for status badges
  getStatusClass(status: string): string {
    switch (status) {
      case 'Applied': return 'badge-info'; // 🔵
      case 'TestPending': return 'badge-warning'; // 🟡
      case 'TestTaken': return 'badge-success'; // 🟢
      case 'Selected': return 'badge-success'; // ✅
      case 'Rejected': return 'badge-danger'; // 🔴
      default: return 'badge-secondary';
    }
  }

  // Helper for UI display text
  getStatusText(status: string): string {
    if (status === 'TestPending') return 'Test Pending';
    if (status === 'TestTaken') return 'Test Taken';
    return status;
  }
}
