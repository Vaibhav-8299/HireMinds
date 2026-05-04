import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    // If the user is already logged in, push them to their respective dashboard
    if (this.authService.isLoggedIn()) {
      const role = this.authService.getUserRole();
      if (role === 'Candidate') {
        this.router.navigate(['/candidate/jobs']);
      } else if (role === 'Recruiter') {
        this.router.navigate(['/recruiter/dashboard']);
      }
    }
  }
}
