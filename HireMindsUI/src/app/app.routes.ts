import { Routes } from '@angular/router';

// Guards
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

// Public Components
import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';

// Candidate Components
import { ProfileComponent } from './pages/candidate/profile/profile.component';
import { JobBoardComponent } from './pages/candidate/job-board/job-board.component';
import { MyApplicationsComponent } from './pages/candidate/my-applications/my-applications.component';
import { TakeTestComponent } from './pages/candidate/take-test/take-test.component';
import { ResultComponent } from './pages/candidate/result/result.component';

// Recruiter Components
import { DashboardComponent } from './pages/recruiter/dashboard/dashboard.component';
import { CreateTestComponent } from './pages/recruiter/create-test/create-test.component';
import { PostJobComponent } from './pages/recruiter/post-job/post-job.component';
import { MyJobsComponent } from './pages/recruiter/my-jobs/my-jobs.component';
import { ViewApplicationsComponent } from './pages/recruiter/view-applications/view-applications.component';
import { MyTestsComponent } from './pages/recruiter/my-tests/my-tests.component';
import { EditTestComponent } from './pages/recruiter/edit-test/edit-test.component';

// Landing Component
import { LandingComponent } from './pages/public/landing/landing.component';

export const routes: Routes = [
  // ==========================================
  // Public Routes
  // ==========================================
  { path: '', component: LandingComponent, title: 'HireMinds | Smart Interview Platform' },
  { path: 'login', component: LoginComponent, title: 'Login | HireMinds' },
  { path: 'register', component: RegisterComponent, title: 'Register | HireMinds' },

  // ==========================================
  // Candidate Routes (Protected)
  // ==========================================
  { 
    path: 'candidate/profile', 
    component: ProfileComponent, 
    canActivate: [authGuard, roleGuard('Candidate')],
    title: 'My Profile | HireMinds'
  },
  { 
    path: 'candidate/jobs', 
    component: JobBoardComponent, 
    canActivate: [authGuard, roleGuard('Candidate')],
    title: 'Job Board | HireMinds'
  },
  { 
    path: 'candidate/applications', 
    component: MyApplicationsComponent, 
    canActivate: [authGuard, roleGuard('Candidate')],
    title: 'My Applications | HireMinds'
  },
  { 
    path: 'candidate/test/:applicationId', 
    component: TakeTestComponent, 
    canActivate: [authGuard, roleGuard('Candidate')],
    title: 'Take Assessment | HireMinds'
  },
  { 
    path: 'candidate/result/:applicationId', 
    component: ResultComponent, 
    canActivate: [authGuard, roleGuard('Candidate')],
    title: 'Assessment Results | HireMinds'
  },

  // ==========================================
  // Recruiter Routes (Protected)
  // ==========================================
  { 
    path: 'recruiter/dashboard', 
    component: DashboardComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'Recruiter Dashboard | HireMinds'
  },
  { 
    path: 'recruiter/create-test', 
    component: CreateTestComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'Create Assessment | HireMinds'
  },
  { 
    path: 'recruiter/post-job', 
    component: PostJobComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'Post New Job | HireMinds'
  },
  { 
    path: 'recruiter/my-jobs', 
    component: MyJobsComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'My Posted Jobs | HireMinds'
  },
  { 
    path: 'recruiter/applications/:jobId', 
    component: ViewApplicationsComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'View Candidates | HireMinds'
  },

  { 
    path: 'recruiter/my-tests', 
    component: MyTestsComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'My Tests | HireMinds'
  },
  { 
    path: 'recruiter/edit-test/:id', 
    component: EditTestComponent, 
    canActivate: [authGuard, roleGuard('Recruiter')],
    title: 'Edit Assessment | HireMinds'
  },

  // ==========================================
  // Fallback Routes
  // ==========================================
  { path: '**', redirectTo: '' }
];
