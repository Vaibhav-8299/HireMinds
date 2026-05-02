import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationResponse } from '../../../core/models/interfaces';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  authService = inject(AuthService);
  applicationService = inject(ApplicationService);
  router = inject(Router);

  userName = this.authService.getUserName() || 'Candidate';
  isCandidate = this.authService.getUserRole() === 'Candidate';
  
  notifications = signal<ApplicationResponse[]>([]);
  showDropdown = signal<boolean>(false);

  ngOnInit() {
    if (this.isCandidate) {
      this.fetchNotifications();
      // Optional: Polling every 60 seconds
      setInterval(() => this.fetchNotifications(), 60000);
    }
  }

  fetchNotifications() {
    this.applicationService.getNotifications().subscribe({
      next: (data) => this.notifications.set(data),
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  toggleDropdown() {
    this.showDropdown.set(!this.showDropdown());
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
