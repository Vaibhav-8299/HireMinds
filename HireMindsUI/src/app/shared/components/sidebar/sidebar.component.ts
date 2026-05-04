import { Component, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  authService = inject(AuthService);
  router = inject(Router);

  userName = this.authService.getUserName() || 'Recruiter';
  // Start expanded only if screen is larger than mobile/tablet breakpoint
  isExpanded = signal<boolean>(window.innerWidth > 992);

  toggleSidebar() {
    this.isExpanded.update(v => !v);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

