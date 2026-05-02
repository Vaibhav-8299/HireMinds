import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

import { TestService } from '../../../core/services/test.service';
import { Test } from '../../../core/models/interfaces';

@Component({
  selector: 'app-my-tests',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-tests.component.html',
  styleUrl: './my-tests.component.css',
  providers: [DatePipe]
})
export class MyTestsComponent implements OnInit {
  private testService = inject(TestService);
  private router = inject(Router);
  private datePipe = inject(DatePipe);

  tests = signal<Test[]>([]);
  isLoading = signal<boolean>(true);
  toastMessage = signal<string>('');

  ngOnInit() {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state && nav.extras.state['message']) {
      this.toastMessage.set(nav.extras.state['message']);
      setTimeout(() => this.toastMessage.set(''), 3000);
    }

    this.fetchTests();
  }

  fetchTests() {
    this.testService.getMyTests().subscribe({
      next: (testsData) => {
        this.tests.set(testsData);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  formatDate(dateStr: string): string {
    return this.datePipe.transform(dateStr, 'mediumDate') || '';
  }
}
