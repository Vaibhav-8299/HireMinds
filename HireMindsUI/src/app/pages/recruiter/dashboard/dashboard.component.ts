import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { JobService } from '../../../core/services/job.service';
import { TestService } from '../../../core/services/test.service';
import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationResponse } from '../../../core/models/interfaces';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  jobService = inject(JobService);
  testService = inject(TestService);
  applicationService = inject(ApplicationService);

  userName = this.authService.getUserName();
  isLoading = signal<boolean>(true);

  // Raw Stats Targets
  targetTotalJobs = 0;
  targetActiveTests = 0;
  targetTotalApps = 0;
  targetShortlisted = 0;

  // Animated Display Stats
  displayTotalJobs = signal<number>(0);
  displayActiveTests = signal<number>(0);
  displayTotalApps = signal<number>(0);
  displayShortlisted = signal<number>(0);

  // Recent Applications
  recentApplications = signal<ApplicationResponse[]>([]);

  ngOnInit() {
    this.fetchDashboardData();
  }

  fetchDashboardData() {
    // 1. Fetch Jobs and Tests in parallel
    forkJoin({
      jobs: this.jobService.getMyJobs(),
      tests: this.testService.getMyTests()
    }).subscribe({
      next: (res) => {
        this.targetTotalJobs = res.jobs.length;
        this.targetActiveTests = res.tests.length;

        // 2. Fetch applications for ALL of this recruiter's jobs
        if (res.jobs.length > 0) {
          const appRequests = res.jobs.map(job => this.applicationService.getApplicationsByJob(job.id));
          
          forkJoin(appRequests).subscribe({
            next: (appArrays) => {
              // Flatten the array of arrays into a single list of all applications
              const allApps = appArrays.flat();
              
              this.targetTotalApps = allApps.length;
              this.targetShortlisted = allApps.filter(a => a.status === 'Selected').length;

              // Sort by date (newest first) and take top 10
              const sorted = allApps.sort((a, b) => new Date(b.appliedAt).getTime() - new Date(a.appliedAt).getTime());
              this.recentApplications.set(sorted.slice(0, 10));

              this.isLoading.set(false);
              this.startCountUpAnimations();
            }
          });
        } else {
          // If no jobs exist, skip app fetching
          this.isLoading.set(false);
          this.startCountUpAnimations();
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ==========================================
  // Number Count-Up Animation
  // ==========================================
  
  startCountUpAnimations() {
    this.animateValue(this.targetTotalJobs, this.displayTotalJobs);
    this.animateValue(this.targetActiveTests, this.displayActiveTests);
    this.animateValue(this.targetTotalApps, this.displayTotalApps);
    this.animateValue(this.targetShortlisted, this.displayShortlisted);
  }

  animateValue(target: number, signalToUpdate: any) {
    if (target === 0) return; // Nothing to animate
    
    let current = 0;
    const duration = 1000; // 1 second animation
    // Determine interval speed based on target size, min 10ms
    const stepTime = Math.max(Math.floor(duration / target), 20); 
    
    const timer = setInterval(() => {
      // Increment faster if the target is high, else by 1
      current += Math.max(1, Math.floor(target / 50)); 
      if (current >= target) {
        signalToUpdate.set(target);
        clearInterval(timer);
      } else {
        signalToUpdate.set(current);
      }
    }, stepTime);
  }

  // ==========================================
  // UI Helpers
  // ==========================================

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

  getStatusText(status: string): string {
    if (status === 'TestPending') return 'Test Pending';
    if (status === 'TestTaken') return 'Test Taken';
    return status;
  }
}
