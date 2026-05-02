import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { JobService } from '../../../core/services/job.service';
import { ApplicationService } from '../../../core/services/application.service';
import { Job } from '../../../core/models/interfaces';

@Component({
  selector: 'app-job-board',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './job-board.component.html',
  styleUrl: './job-board.component.css'
})
export class JobBoardComponent implements OnInit {
  // Services
  private jobService = inject(JobService);
  private applicationService = inject(ApplicationService);

  // Angular 19 Signals for State Management
  jobs = signal<Job[]>([]);
  isLoading = signal<boolean>(true);
  
  // Filter Signals
  searchQuery = signal<string>('');
  locationFilter = signal<string>('');
  companyFilter = signal<string>('');

  // Applied Jobs Tracking (Job ID -> AppliedAt Date)
  appliedJobs = signal<Map<number, Date>>(new Map());

  // Toast State
  toastMessage = signal<string>('');
  toastType = signal<'success' | 'error'>('success');

  // RxJS Subject for debouncing filter inputs
  private filterSubject = new Subject<void>();

  ngOnInit() {
    // 1. Fetch the jobs the candidate has already applied to
    this.applicationService.getMyApplications().subscribe({
      next: (apps) => {
        const appliedMap = new Map<number, Date>();
        apps.forEach(a => {
          appliedMap.set(a.jobId, new Date(a.appliedAt));
        });
        this.appliedJobs.set(appliedMap);
      }
    });

    // 2. Setup debouncer for smooth searching (waits 300ms after user stops typing)
    this.filterSubject.pipe(
      debounceTime(300)
    ).subscribe(() => {
      this.fetchJobs();
    });

    // 3. Initial fetch
    this.fetchJobs();
  }

  // Called whenever an input changes
  onFilterChange() {
    this.filterSubject.next();
  }

  clearFilters() {
    this.searchQuery.set('');
    this.locationFilter.set('');
    this.companyFilter.set('');
    this.fetchJobs();
  }

  fetchJobs() {
    this.isLoading.set(true);
    
    // Call API with current signal values
    this.jobService.getJobs(
      this.searchQuery(), 
      this.locationFilter(), 
      this.companyFilter()
    ).subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.showToast('Failed to load jobs', 'error');
        this.isLoading.set(false);
      }
    });
  }

  applyToJob(jobId: number) {
    this.applicationService.applyToJob(jobId).subscribe({
      next: () => {
        this.showToast('Applied! For giving test go to My Applications tab.', 'success');
        // Update local map to immediately reflect the "Applied" state in the UI
        const updatedMap = new Map(this.appliedJobs());
        updatedMap.set(jobId, new Date());
        this.appliedJobs.set(updatedMap);
      },
      error: (err) => {
        this.showToast(err.error?.message || 'Failed to apply. Check your eligibility.', 'error');
      }
    });
  }

  isCooldownActive(jobId: number): boolean {
    const appliedAt = this.appliedJobs().get(jobId);
    if (!appliedAt) return false;

    // Check if appliedAt is within the last 3 months
    const threeMonthsAgo = new Date();
    threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);
    
    return appliedAt > threeMonthsAgo;
  }

  getEligibilityDate(jobId: number): string {
    const appliedAt = this.appliedJobs().get(jobId);
    if (!appliedAt) return '';

    const eligibleDate = new Date(appliedAt);
    eligibleDate.setMonth(eligibleDate.getMonth() + 3);
    
    return this.formatDate(eligibleDate.toISOString());
  }

  // Helper to show temporary toast messages
  showToast(message: string, type: 'success' | 'error') {
    this.toastMessage.set(message);
    this.toastType.set(type);
    
    // Auto-hide after 3.5 seconds
    setTimeout(() => {
      this.toastMessage.set('');
    }, 3500);
  }

  // Format date nicely
  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  }
}
