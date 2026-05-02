import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormsModule } from '@angular/forms';
import { ProfileService } from '../../../core/services/profile.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileResponse, Certification, Project } from '../../../core/models/interfaces';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);

  profileForm!: FormGroup;
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);
  toastMessage = signal<string>('');
  toastType = signal<string>('success');
  
  // Resume
  resumeUrl = signal<string>('');
  isUploading = signal<boolean>(false);
  resumeFileName = signal<string>('');

  // Skills chips
  skillsList = signal<string[]>([]);
  skillInput = '';

  // Read-only info from JWT
  fullName = '';
  email = '';

  // Degree/Branch options
  degreeOptions = ['B.Tech', 'M.Tech', 'BCA', 'MCA', 'B.Sc', 'M.Sc', 'MBA', 'Other'];
  branchOptions = ['CS', 'IT', 'ECE', 'EE', 'ME', 'CE', 'Other'];

  ngOnInit() {
    this.fullName = this.authService.getUserName() || '';
    this.email = this.authService.getUserEmail() || '';
    this.buildForm();
    this.loadProfile();
  }

  buildForm() {
    this.profileForm = this.fb.group({
      degree: [''],
      branch: [''],
      graduationYear: [null],
      collegeName: [''],
      sgpa: [null],
      tenthSchoolName: [''],
      tenthScore: [null],
      twelfthSchoolName: [''],
      twelfthScore: [null],
      certifications: this.fb.array([]),
      projects: this.fb.array([])
    });
  }

  // FormArray accessors
  get certifications(): FormArray {
    return this.profileForm.get('certifications') as FormArray;
  }
  get projects(): FormArray {
    return this.profileForm.get('projects') as FormArray;
  }

  addCertification() {
    this.certifications.push(this.fb.group({ name: [''], link: [''] }));
  }

  removeCertification(index: number) {
    this.certifications.removeAt(index);
  }

  addProject() {
    this.projects.push(this.fb.group({ title: [''], description: [''] }));
  }

  removeProject(index: number) {
    this.projects.removeAt(index);
  }

  // Skills management
  addSkill(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const val = input.value.trim();
    if (event.key === 'Enter' && val) {
      event.preventDefault();
      const current = this.skillsList();
      if (!current.includes(val)) {
        this.skillsList.set([...current, val]);
      }
      input.value = '';
    }
  }

  removeSkill(skill: string) {
    this.skillsList.set(this.skillsList().filter(s => s !== skill));
  }

  // Resume upload
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || !input.files.length) return;
    const file = input.files[0];

    if (file.size > 5 * 1024 * 1024) {
      this.showToast('File too large. Max 5MB allowed.', 'error');
      return;
    }
    if (file.type !== 'application/pdf') {
      this.showToast('Only PDF files are allowed.', 'error');
      return;
    }

    this.isUploading.set(true);
    this.resumeFileName.set(file.name);
    this.profileService.uploadResume(file).subscribe({
      next: (res: any) => {
        this.resumeUrl.set(res.url);
        this.isUploading.set(false);
        this.showToast('Resume uploaded successfully!', 'success');
      },
      error: () => {
        this.isUploading.set(false);
        this.showToast('Failed to upload resume.', 'error');
      }
    });
  }

  loadProfile() {
    this.profileService.getProfile().subscribe({
      next: (profile: ProfileResponse) => {
        // Patch simple fields
        this.profileForm.patchValue({
          degree: profile.degree || '',
          branch: profile.branch || '',
          graduationYear: profile.graduationYear || null,
          collegeName: profile.collegeName || '',
          sgpa: profile.sgpa || null,
          tenthSchoolName: profile.tenthSchoolName || '',
          tenthScore: profile.tenthScore || null,
          twelfthSchoolName: profile.twelfthSchoolName || '',
          twelfthScore: profile.twelfthScore || null
        });

        // Skills
        if (profile.skills) {
          this.skillsList.set(profile.skills.split(',').map(s => s.trim()).filter(s => s));
        }

        // Resume
        if (profile.resumeUrl) {
          this.resumeUrl.set(profile.resumeUrl);
        }

        // Certifications (JSON)
        if (profile.certifications) {
          try {
            const certs: Certification[] = JSON.parse(profile.certifications);
            certs.forEach(c => {
              this.certifications.push(this.fb.group({ name: [c.name], link: [c.link] }));
            });
          } catch {}
        }

        // Projects (JSON)
        if (profile.projects) {
          try {
            const projs: Project[] = JSON.parse(profile.projects);
            projs.forEach(p => {
              this.projects.push(this.fb.group({ title: [p.title], description: [p.description] }));
            });
          } catch {}
        }

        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  saveProfile() {
    if (this.isSaving()) return;
    this.isSaving.set(true);

    const formVal = this.profileForm.value;

    const dto = {
      degree: formVal.degree || '',
      branch: formVal.branch || '',
      graduationYear: formVal.graduationYear || 0,
      skills: this.skillsList().join(','),
      collegeName: formVal.collegeName || '',
      sgpa: formVal.sgpa || null,
      tenthSchoolName: formVal.tenthSchoolName || '',
      tenthScore: formVal.tenthScore || null,
      twelfthSchoolName: formVal.twelfthSchoolName || '',
      twelfthScore: formVal.twelfthScore || null,
      certifications: JSON.stringify(formVal.certifications || []),
      projects: JSON.stringify(formVal.projects || []),
      resumeUrl: this.resumeUrl() || ''
    };

    this.profileService.updateProfile(dto).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showToast('Profile saved successfully!', 'success');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('Failed to save profile.', 'error');
      }
    });
  }

  showToast(msg: string, type: string) {
    this.toastMessage.set(msg);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(''), 3000);
  }
}
