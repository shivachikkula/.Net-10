import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { Student, StudentFormValue } from '../../models/student';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-student-form-dialog',
  styleUrl: './student-form-dialog.css',
  templateUrl: './student-form-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentFormDialog {
  readonly student = input<Student | null>(null);
  readonly error = input<string | null>(null);

  readonly saved = output<StudentFormValue>();
  readonly closed = output<void>();

  private readonly formBuilder = new FormBuilder().nonNullable;

  readonly form = this.formBuilder.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    dateOfBirth: ['', [Validators.required]],
    department: [''],
    gpa: [0, [Validators.required, Validators.min(0), Validators.max(4)]],
  });

  constructor() {
    effect(() => {
      const student = this.student();

      this.form.reset({
        firstName: student?.firstName ?? '',
        lastName: student?.lastName ?? '',
        email: student?.email ?? '',
        dateOfBirth: student?.dateOfBirth ?? '',
        department: student?.department ?? '',
        gpa: student?.gpa ?? 0,
      });
    });
  }

  get isEditMode(): boolean {
    return this.student() !== null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const department = value.department.trim();

    this.saved.emit({
      ...value,
      department: department.length === 0 ? null : department,
    });
  }

  cancel(): void {
    this.closed.emit();
  }
}
