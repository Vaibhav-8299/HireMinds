import { Component, OnInit, OnDestroy, signal, computed, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TestService } from '../../../core/services/test.service';
import { ApplicationService } from '../../../core/services/application.service';
import { SubmissionService } from '../../../core/services/submission.service';
import { TestResponse, Question } from '../../../core/models/interfaces';

@Component({
  selector: 'app-take-test',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './take-test.component.html',
  styleUrl: './take-test.component.css'
})
export class TakeTestComponent implements OnInit, OnDestroy {
  // Routing & Services
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private testService = inject(TestService);
  private applicationService = inject(ApplicationService);
  private submissionService = inject(SubmissionService);

  // State Signals
  applicationId = signal<number>(0);
  testData = signal<TestResponse | null>(null);
  isLoading = signal<boolean>(true);
  hasStarted = signal<boolean>(false);
  
  // Navigation State
  currentQuestionIndex = signal<number>(0);
  
  // Storage for Answers: Map<QuestionId, SelectedOptionId>
  answers = signal<Map<number, number>>(new Map());

  // Timer State
  remainingSeconds = signal<number>(0);
  private timerInterval: any;

  // UI Modals
  showSubmitModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  // Anti-Cheat State
  switchCount = 0;
  lastViolationTime = 0;

  // Derived Values (Computed Signals)
  currentQuestion = computed<Question | null>(() => {
    const test = this.testData();
    if (!test || !test.questions.length) return null;
    return test.questions[this.currentQuestionIndex()];
  });

  isLastQuestion = computed(() => {
    const test = this.testData();
    if (!test) return false;
    return this.currentQuestionIndex() === test.questions.length - 1;
  });

  isFirstQuestion = computed(() => this.currentQuestionIndex() === 0);

  // ==========================================
  // Initialization & Data Loading
  // ==========================================

  ngOnInit() {
    const appIdParam = this.route.snapshot.paramMap.get('applicationId');
    if (appIdParam) {
      const id = parseInt(appIdParam, 10);
      this.applicationId.set(id);

      // Anti-cheat: Check if they refreshed during an active test
      if (localStorage.getItem(`test_started_${id}`)) {
        alert('🚫 CHEAT DETECTED: You refreshed the page during an active test. Your test has been automatically submitted.');
        localStorage.removeItem(`test_started_${id}`);
        this.forceSubmit(); // Submit whatever empty/current state exists
        return;
      }

      this.loadTestData();
    }
  }

  startTest() {
    this.hasStarted.set(true);
    // Set flag that test has started
    localStorage.setItem(`test_started_${this.applicationId()}`, 'true');
    // Start timer
    if (this.testData()) {
      this.startTimer(this.testData()!.timeLimitMinutes * 60);
    }
  }

  // ==========================================
  // Anti-Cheat Listeners
  // ==========================================

  @HostListener('window:visibilitychange')
  onVisibilityChange() {
    if (document.visibilityState === 'hidden') {
      this.handleViolation();
    }
  }

  @HostListener('window:blur')
  onWindowBlur() {
    this.handleViolation();
  }

  handleViolation() {
    // If the test is already submitting or finished, don't trigger.
    if (this.isSubmitting() || this.remainingSeconds() <= 0 || !this.testData()) return;

    const now = Date.now();
    if (now - this.lastViolationTime < 2000) {
      // Ignore rapid consecutive events from a single switch action
      return; 
    }
    this.lastViolationTime = now;

    this.switchCount++;
    if (this.switchCount === 1) {
      alert('⚠️ WARNING: Tab or screen switching is strictly prohibited! If you switch again, your test will be automatically submitted.');
    } else if (this.switchCount >= 2) {
      alert('🚫 CHEAT DETECTED: You have violated the anti-cheat rules by switching screens again. Your test is being automatically submitted.');
      this.forceSubmit();
    }
  }

  ngOnDestroy() {
    // Prevent memory leaks
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  loadTestData() {
    // 1. Get Application Details to find the Test ID
    this.applicationService.getApplicationById(this.applicationId()).subscribe({
      next: (app) => {
        if (!app.testId) {
          alert('Error: Test ID not found for this application.');
          this.router.navigate(['/candidate/applications']);
          return;
        }

        // 2. Fetch the actual Test Questions
        this.testService.getTestById(app.testId).subscribe({
          next: (test) => {
            this.testData.set(test);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
        });
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ==========================================
  // Timer Logic
  // ==========================================

  startTimer(totalSeconds: number) {
    this.remainingSeconds.set(totalSeconds);
    
    this.timerInterval = setInterval(() => {
      const current = this.remainingSeconds();
      if (current <= 0) {
        clearInterval(this.timerInterval);
        this.forceSubmit(); // Time's up!
      } else {
        this.remainingSeconds.set(current - 1);
      }
    }, 1000);
  }

  get formattedTime(): string {
    const totalSeconds = this.remainingSeconds();
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }

  get isTimeRunningOut(): boolean {
    // Less than 2 minutes
    return this.remainingSeconds() < 120 && this.remainingSeconds() > 0;
  }

  // ==========================================
  // Interaction Logic
  // ==========================================

  selectOption(questionId: number, optionId: number) {
    // Create a new Map to trigger signal reactivity
    const newAnswers = new Map(this.answers());
    newAnswers.set(questionId, optionId);
    this.answers.set(newAnswers);
  }

  nextQuestion() {
    if (!this.isLastQuestion()) {
      this.currentQuestionIndex.update(i => i + 1);
    }
  }

  prevQuestion() {
    if (!this.isFirstQuestion()) {
      this.currentQuestionIndex.update(i => i - 1);
    }
  }

  goToQuestion(index: number) {
    this.currentQuestionIndex.set(index);
  }

  // Check if a specific question has been answered (for the dot indicators)
  hasAnswered(questionId: number): boolean {
    return this.answers().has(questionId);
  }

  // ==========================================
  // Submission Logic
  // ==========================================

  openSubmitModal() {
    this.showSubmitModal.set(true);
  }

  closeSubmitModal() {
    this.showSubmitModal.set(false);
  }

  forceSubmit() {
    this.showSubmitModal.set(false);
    this.submitTest();
  }

  submitTest() {
    if (this.isSubmitting()) return;
    this.isSubmitting.set(true);

    // Stop timer
    if (this.timerInterval) clearInterval(this.timerInterval);

    // Convert Map into the DTO array format expected by the backend
    const answerArray = Array.from(this.answers().entries()).map(([qId, oId]) => ({
      questionId: qId,
      selectedOptionId: oId
    }));

    const payload = { answers: answerArray };

    this.submissionService.submitTest(this.applicationId(), payload).subscribe({
      next: () => {
        // Clear anti-cheat flag
        localStorage.removeItem(`test_started_${this.applicationId()}`);
        // Successfully graded! Redirect to results page
        this.router.navigate(['/candidate/result', this.applicationId()]);
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to submit test.');
        this.isSubmitting.set(false);
      }
    });
  }
}
