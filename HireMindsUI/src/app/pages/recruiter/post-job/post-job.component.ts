import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { JobService } from '../../../core/services/job.service';
import { TestService } from '../../../core/services/test.service';
import { Test } from '../../../core/models/interfaces';

@Component({
  selector: 'app-post-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './post-job.component.html',
  styleUrl: './post-job.component.css'
})
export class PostJobComponent implements OnInit {
  private fb = inject(FormBuilder);
  private jobService = inject(JobService);
  private testService = inject(TestService);
  private router = inject(Router);

  jobForm!: FormGroup;
  
  availableTests = signal<Test[]>([]);
  isLoadingTests = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string>('');

  availableDegrees = ['Any', 'B.Tech', 'M.Tech', 'BCA', 'MCA'];
  availableBranches = ['Any', 'CS', 'IT', 'ECE', 'EE', 'ME', 'Civil'];

  ngOnInit() {
    // Initialize Form
    this.jobForm = this.fb.group({
      title: ['', Validators.required],
      companyName: ['', Validators.required],
      location: ['', Validators.required],
      description: ['', Validators.required],
      requiredDegree: [['Any'], Validators.required], // Array of selected degrees
      requiredBranch: [['Any'], Validators.required], // Array of selected branches
      testId: ['', Validators.required],
      minBTechScore: [null],
      min10thScore: [null],
      min12thScore: [null]
    });

    // Fetch tests created by this recruiter to link to the job
    this.testService.getMyTests().subscribe({
      next: (tests) => {
        this.availableTests.set(tests);
        this.isLoadingTests.set(false);
      },
      error: () => {
        this.isLoadingTests.set(false);
        this.errorMessage.set('Failed to load available tests. Please create a test first.');
      }
    });
  }

  // Toggle logic for Degree Pills
  toggleDegree(degree: string) {
    let current: string[] = this.jobForm.value.requiredDegree || [];
    if (degree === 'Any') {
      current = ['Any'];
    } else {
      current = current.filter(d => d !== 'Any');
      if (current.includes(degree)) {
        current = current.filter(d => d !== degree);
      } else {
        current.push(degree);
      }
      if (current.length === 0) current = ['Any'];
    }
    this.jobForm.patchValue({ requiredDegree: current });
  }

  // Toggle logic for Branch Pills
  toggleBranch(branch: string) {
    let current: string[] = this.jobForm.value.requiredBranch || [];
    if (branch === 'Any') {
      current = ['Any'];
    } else {
      current = current.filter(b => b !== 'Any');
      if (current.includes(branch)) {
        current = current.filter(b => b !== branch);
      } else {
        current.push(branch);
      }
      if (current.length === 0) current = ['Any'];
    }
    this.jobForm.patchValue({ requiredBranch: current });
  }

  isDegreeSelected(degree: string): boolean {
    return (this.jobForm.value.requiredDegree || []).includes(degree);
  }

  isBranchSelected(branch: string): boolean {
    return (this.jobForm.value.requiredBranch || []).includes(branch);
  }

  get f() { return this.jobForm.controls; }

  onSubmit() {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    // Clone payload and format the arrays into comma-separated strings
    const payload = { ...this.jobForm.value };
    payload.requiredDegree = payload.requiredDegree.join(', ');
    payload.requiredBranch = payload.requiredBranch.join(', ');
    
    // Ensure numeric fields are correctly typed
    payload.minBTechScore = payload.minBTechScore ? Number(payload.minBTechScore) : null;
    payload.min10thScore = payload.min10thScore ? Number(payload.min10thScore) : null;
    payload.min12thScore = payload.min12thScore ? Number(payload.min12thScore) : null;

    this.jobService.createJob(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        // Note: Passing state via router is a clean way to show toasts on the next page
        this.router.navigate(['/recruiter/my-jobs'], { state: { message: 'Job posted successfully!' }});
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to post job.');
      }
    });
  }
}
