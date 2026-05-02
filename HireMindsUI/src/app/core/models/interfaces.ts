// ==========================================================================
// Authentication Models
// ==========================================================================

// Payload sent when a user logs in
export interface LoginRequest {
  email: string;
  password?: string; // Optional so we don't accidentally display it
}

// Payload sent when a new user registers
export interface RegisterRequest {
  fullName: string;
  email: string;
  password?: string;
  role: 'Candidate' | 'Recruiter';
}

// Data returned after a successful login
export interface LoginResponse {
  token: string;
  role: 'Candidate' | 'Recruiter';
  fullName: string;
}

// ==========================================================================
// User Models
// ==========================================================================

// Represents the authenticated user stored in local state
export interface User {
  id: number;
  fullName: string;
  email: string;
  role: 'Candidate' | 'Recruiter';
}

// ==========================================================================
// Profile Models
// ==========================================================================

// Payload sent to update a candidate's profile
export interface ProfileDto {
  degree: string;
  branch: string;
  graduationYear: number;
  skills: string;
  collegeName: string;
  sgpa: number | null;
  tenthSchoolName: string;
  tenthScore: number | null;
  twelfthSchoolName: string;
  twelfthScore: number | null;
  certifications: string;
  projects: string;
  resumeUrl: string;
}

// Complete profile data returned from the API
export interface ProfileResponse {
  id: number;
  fullName: string;
  email: string;
  degree: string;
  branch: string;
  graduationYear: number;
  skills: string;
  collegeName: string;
  sgpa: number | null;
  tenthSchoolName: string;
  tenthScore: number | null;
  twelfthSchoolName: string;
  twelfthScore: number | null;
  resumeUrl: string;
  certifications: string;
  projects: string;
}

// Helper interfaces for structured JSON fields
export interface Certification {
  name: string;
  link: string;
}

export interface Project {
  title: string;
  description: string;
}

// ==========================================================================
// Job Models
// ==========================================================================

// Represents a Job listing returned from the API
export interface Job {
  id: number;
  title: string;
  description: string;
  companyName: string;
  location: string;
  requiredDegree?: string;
  requiredBranch?: string;
  testId?: number;
  minBTechScore?: number;
  min10thScore?: number;
  min12thScore?: number;
  isActive: boolean;
  postedAt: string;
  recruiterName: string;
  applicationCount: number;
}

// Payload sent by a recruiter to post a new job
export interface CreateJobRequest {
  title: string;
  description: string;
  companyName: string;
  location: string;
  requiredDegree?: string;
  requiredBranch?: string;
  testId?: number;
  minBTechScore?: number;
  min10thScore?: number;
  min12thScore?: number;
}

// ==========================================================================
// Test & Question Models
// ==========================================================================

// Represents a basic test (without questions)
export interface Test {
  id: number;
  title: string;
  timeLimitMinutes: number;
  createdAt: string;
  recruiterName: string;
}

// Represents a complete test including all questions and options
export interface TestResponse extends Test {
  questions: Question[];
}

// Represents a single question in a test
export interface Question {
  id: number;
  text: string;
  topic: string;
  difficulty: string;
  options: Option[];
}

// Represents a single option for a question
export interface Option {
  id: number;
  text: string;
  isCorrect?: boolean; // Only available when recruiter views it, hidden from candidate
}

// Payload sent to create a new empty test
export interface CreateTestRequest {
  title: string;
  timeLimitMinutes: number;
}

// Payload sent to add a question to an existing test
export interface AddQuestionRequest {
  text: string;
  topic: string;
  difficulty: string;
  options: { text: string; isCorrect: boolean }[];
}

// ==========================================================================
// Application Models
// ==========================================================================

// Base application representation
export interface Application {
  id: number;
  candidateId: number;
  jobId: number;
  status: string;
  appliedAt: string;
}

// Detailed application data returned from API (includes Candidate Profile and Job info)
export interface ApplicationResponse extends Application {
  candidateName: string;
  jobTitle: string;
  companyName: string;
  location: string;
  score?: number;
  totalQuestions?: number;
  aiFeedback?: string;
  selectionMessage?: string;
  testTakenAt?: string;
  testId?: number;
  
  // Profile info visible to recruiter
  degree?: string;
  branch?: string;
  skills?: string;
  collegeName?: string;
  sgpa?: number;
  tenthSchoolName?: string;
  tenthScore?: number;
  twelfthSchoolName?: string;
  twelfthScore?: number;
  resumeUrl?: string;
  certifications?: string;
  projects?: string;
}

// ==========================================================================
// Test Submission Models
// ==========================================================================

// Represents one answer selected by the candidate
export interface SubmitAnswer {
  questionId: number;
  selectedOptionId: number;
}

// Payload sent when a candidate submits the entire test
export interface SubmitTestRequest {
  answers: SubmitAnswer[];
}

// Detailed breakdown of the candidate's test score
export interface TestResult {
  score: number;
  totalQuestions: number;
  percentage: number;
  aiFeedback?: string;
  status?: string;
  topicWiseScore: {
    [topic: string]: {
      correct: number;
      total: number;
      percentage: number;
    }
  };
}
