import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationResponse } from '../../../core/models/interfaces';

@Component({
  selector: 'app-view-applications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './view-applications.component.html',
  styleUrl: './view-applications.component.css'
})
export class ViewApplicationsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private applicationService = inject(ApplicationService);

  jobId = signal<number>(0);
  jobTitle = signal<string>('Loading...');
  applications = signal<ApplicationResponse[]>([]);
  isLoading = signal<boolean>(true);

  // Modals State
  selectedAppForView = signal<ApplicationResponse | null>(null);
  
  showConfirmModal = signal<boolean>(false);
  confirmAction = signal<'Selected' | 'Rejected' | null>(null);
  confirmAppId = signal<number | null>(null);
  selectionMessage = signal<string>(''); // Added for the recruiter's message
  isUpdating = signal<boolean>(false);

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('jobId');
    if (idParam) {
      this.jobId.set(parseInt(idParam, 10));
      this.loadApplications();
    }
  }

  loadApplications() {
    this.applicationService.getApplicationsByJob(this.jobId()).subscribe({
      next: (apps) => {
        this.applications.set(apps);
        // Extract job title from first application if available
        if (apps.length > 0) {
          this.jobTitle.set(apps[0].jobTitle);
        } else {
          this.jobTitle.set('this Job');
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ==========================================
  // Status Update Actions
  // ==========================================

  initiateStatusUpdate(appId: number, status: 'Selected' | 'Rejected') {
    this.confirmAppId.set(appId);
    this.confirmAction.set(status);
    this.selectionMessage.set(''); // Reset message when opening modal
    this.showConfirmModal.set(true);
  }

  // Helper method to bind textarea
  updateMessage(event: Event) {
    const val = (event.target as HTMLTextAreaElement).value;
    this.selectionMessage.set(val);
  }

  confirmUpdate() {
    const appId = this.confirmAppId();
    const status = this.confirmAction();
    const message = this.selectionMessage();
    
    if (!appId || !status) return;

    this.isUpdating.set(true);
    
    this.applicationService.updateStatus(appId, status, message).subscribe({
      next: () => {
        // Update local state without full refetch
        this.applications.update(apps => 
          apps.map(app => app.id === appId ? { ...app, status: status, selectionMessage: message } : app)
        );
        this.closeConfirmModal();
        this.isUpdating.set(false);
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to update status.');
        this.isUpdating.set(false);
      }
    });
  }

  closeConfirmModal() {
    this.showConfirmModal.set(false);
    this.confirmAppId.set(null);
    this.confirmAction.set(null);
    this.selectionMessage.set('');
  }

  // ==========================================
  // View Details Modal
  // ==========================================

  openViewModal(app: ApplicationResponse) {
    this.selectedAppForView.set(app);
  }

  closeViewModal() {
    this.selectedAppForView.set(null);
  }

  // ==========================================
  // UI Helpers
  // ==========================================

  getScorePercentage(app: ApplicationResponse): number {
    const score = app.score ?? 0;
    const total = app.totalQuestions ?? 0;
    if (total === 0) return 0;
    return (score / total) * 100;
  }

  getScoreColorClass(percentage: number): string {
    if (percentage >= 70) return 'bg-green';
    if (percentage >= 50) return 'bg-yellow';
    return 'bg-red';
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Applied': return 'badge-info';
      case 'TestPending': return 'badge-warning';
      case 'TestTaken': return 'badge-success';
      case 'Selected': return 'badge-success';
      case 'Rejected': return 'badge-danger';
      default: return 'badge-secondary';
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString();
  }

  parseJson(jsonStr: string): any[] {
    try {
      return JSON.parse(jsonStr);
    } catch {
      return [];
    }
  }
}
