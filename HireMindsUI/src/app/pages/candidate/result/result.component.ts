import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { SubmissionService } from '../../../core/services/submission.service';
import { TestResult } from '../../../core/models/interfaces';

@Component({
  selector: 'app-result',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './result.component.html',
  styleUrl: './result.component.css'
})
export class ResultComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private submissionService = inject(SubmissionService);

  resultData = signal<TestResult | null>(null);
  isLoading = signal<boolean>(true);

  // AI Typing Effect State
  displayedFeedback = signal<string>('');
  private typeInterval: any;

  // Topics converted from object to array for easy iteration
  topicsList = computed(() => {
    const data = this.resultData();
    if (!data || !data.topicWiseScore) return [];
    
    return Object.keys(data.topicWiseScore).map(topicName => {
      const stats = data.topicWiseScore[topicName];
      return {
        name: topicName,
        correct: stats.correct,
        total: stats.total,
        percentage: stats.percentage,
        colorClass: this.getColorClass(stats.percentage)
      };
    });
  });

  ngOnInit() {
    const appId = this.route.snapshot.paramMap.get('applicationId');
    if (appId) {
      this.submissionService.getResult(parseInt(appId, 10)).subscribe({
        next: (data) => {
          this.resultData.set(data);
          this.isLoading.set(false);

          // Start typing animation if feedback exists
          if (data.aiFeedback) {
            this.startTypingEffect(data.aiFeedback);
          }
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    }
  }

  // ==========================================
  // Visual Helpers
  // ==========================================

  getColorClass(percentage: number): string {
    if (percentage >= 70) return 'green';
    if (percentage >= 50) return 'yellow';
    return 'red';
  }

  // Gets the exact color value for the circular gauge
  getScoreHexColor(percentage: number): string {
    if (percentage >= 70) return '#22c55e'; // success green
    if (percentage >= 50) return '#f59e0b'; // warning yellow
    return '#ef4444'; // danger red
  }

  // ==========================================
  // AI Typing Effect Logic
  // ==========================================

  startTypingEffect(fullText: string) {
    let currentIndex = 0;
    this.displayedFeedback.set('');

    this.typeInterval = setInterval(() => {
      if (currentIndex < fullText.length) {
        // Append the next character
        this.displayedFeedback.update(current => current + fullText.charAt(currentIndex));
        currentIndex++;
      } else {
        // Finished typing
        clearInterval(this.typeInterval);
      }
    }, 15); // 15ms per character creates a fast but readable "AI typing" feel
  }

  // Clean up interval if component is destroyed before typing finishes
  ngOnDestroy() {
    if (this.typeInterval) {
      clearInterval(this.typeInterval);
    }
  }
}
