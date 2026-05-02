import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { TestService } from '../../../core/services/test.service';

@Component({
  selector: 'app-create-test',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-test.component.html',
  styleUrl: './create-test.component.css'
})
export class CreateTestComponent implements OnInit {
  private fb = inject(FormBuilder);
  private testService = inject(TestService);
  private router = inject(Router);

  testForm!: FormGroup;
  
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string>('');
  showAddQuestionForm = signal<boolean>(false);

  // Temporary standalone form for adding a single question before pushing to array
  newQuestionForm!: FormGroup;

  ngOnInit() {
    this.testForm = this.fb.group({
      title: ['', Validators.required],
      timeLimitMinutes: [30, [Validators.required, Validators.min(1)]],
      questions: this.fb.array([], Validators.minLength(1)) // Ensure at least 1 question
    });

    this.initNewQuestionForm();
  }

  // Getters for template
  get questionsFormArray() {
    return this.testForm.get('questions') as FormArray;
  }

  get f() { return this.testForm.controls; }
  get nq() { return this.newQuestionForm.controls; }

  // Initialize the temporary form for adding a question
  initNewQuestionForm() {
    this.newQuestionForm = this.fb.group({
      text: ['', Validators.required],
      topic: ['', Validators.required],
      difficulty: ['Medium', Validators.required],
      optionA: ['', Validators.required],
      optionB: ['', Validators.required],
      optionC: ['', Validators.required],
      optionD: ['', Validators.required],
      correctAnswer: ['A', Validators.required]
    });
  }

  toggleAddQuestionForm() {
    this.showAddQuestionForm.update(val => !val);
  }

  // Adds the configured question to the main FormArray
  addQuestionToList() {
    if (this.newQuestionForm.invalid) {
      this.newQuestionForm.markAllAsTouched();
      return;
    }

    const qValues = this.newQuestionForm.value;
    
    // Map form structure to expected DTO structure
    const questionGroup = this.fb.group({
      text: [qValues.text],
      topic: [qValues.topic],
      difficulty: [qValues.difficulty],
      options: this.fb.array([
        this.fb.group({ text: [qValues.optionA], isCorrect: [qValues.correctAnswer === 'A'] }),
        this.fb.group({ text: [qValues.optionB], isCorrect: [qValues.correctAnswer === 'B'] }),
        this.fb.group({ text: [qValues.optionC], isCorrect: [qValues.correctAnswer === 'C'] }),
        this.fb.group({ text: [qValues.optionD], isCorrect: [qValues.correctAnswer === 'D'] })
      ])
    });

    this.questionsFormArray.push(questionGroup);
    
    // Reset temporary form and hide
    this.initNewQuestionForm();
    this.showAddQuestionForm.set(false);
  }

  removeQuestion(index: number) {
    this.questionsFormArray.removeAt(index);
  }

  // Final Submission
  onSubmit() {
    if (this.testForm.invalid || this.questionsFormArray.length === 0) {
      this.testForm.markAllAsTouched();
      if (this.questionsFormArray.length === 0) {
        this.errorMessage.set('You must add at least one question to the test.');
      }
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const testPayload = {
      title: this.testForm.value.title,
      timeLimitMinutes: this.testForm.value.timeLimitMinutes
    };

    // 1. Create the Test first
    this.testService.createTest(testPayload).subscribe({
      next: (response) => {
        // Backend returns { message: "...", testId: 123 }
        const testId = response.testId || response.id; // Fallback just in case
        
        if (!testId) {
            this.errorMessage.set('Test created, but failed to retrieve Test ID.');
            this.isSubmitting.set(false);
            return;
        }

        // 2. Add all questions concurrently using forkJoin
        const questionRequests = this.questionsFormArray.value.map((q: any) => 
          this.testService.addQuestion(testId, q)
        );

        if (questionRequests.length > 0) {
          forkJoin(questionRequests).subscribe({
            next: () => {
              this.isSubmitting.set(false);
              this.router.navigate(['/recruiter/dashboard']);
            },
            error: (err) => {
              this.errorMessage.set('Test created, but some questions failed to save.');
              this.isSubmitting.set(false);
            }
          });
        } else {
          this.isSubmitting.set(false);
          this.router.navigate(['/recruiter/dashboard']);
        }
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to create test.');
        this.isSubmitting.set(false);
      }
    });
  }
}
