import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { JobService } from '../../../core/services/job.service';
import { ApplicationService } from '../../../core/services/application.service';
import { Job } from '../../../core/models/interfaces';

// Extension interface to hold the applicant count for the UI
interface JobWithCount extends Job {
  applicantCount: number;
}

@Component({
  selector: 'app-my-jobs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-jobs.component.html',
  styleUrl: './my-jobs.component.css'
})
export class MyJobsComponent implements OnInit {
  private jobService = inject(JobService);
  private applicationService = inject(ApplicationService);
  private router = inject(Router);

  jobs = signal<JobWithCount[]>([]);
  isLoading = signal<boolean>(true);
  toastMessage = signal<string>('');

  ngOnInit() {
    // Check if we came from Post Job with a success message
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state && nav.extras.state['message']) {
      this.toastMessage.set(nav.extras.state['message']);
      setTimeout(() => this.toastMessage.set(''), 3000);
    }

    this.fetchJobs();
  }

  fetchJobs() {
    this.jobService.getMyJobs().subscribe({
      next: (jobData) => {
        if (jobData.length === 0) {
          this.isLoading.set(false);
          return;
        }

        // Fetch application count for each job
        const appRequests = jobData.map(job => this.applicationService.getApplicationsByJob(job.id));
        
        forkJoin(appRequests).subscribe({
          next: (appArrays) => {
            const enrichedJobs = jobData.map((job, index) => ({
              ...job,
              applicantCount: appArrays[index].length
            }));
            
            this.jobs.set(enrichedJobs);
            this.isLoading.set(false);
          },
          error: () => {
            // Fallback if app count fails
            this.jobs.set(jobData.map(j => ({ ...j, applicantCount: 0 })));
            this.isLoading.set(false);
          }
        });
      },
      error: () => this.isLoading.set(false)
    });
  }
}
