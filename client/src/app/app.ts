import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { StudentFormDialog } from './components/student-form-dialog/student-form-dialog';
import { StudentGrid } from './components/student-grid/student-grid';
import { Student, StudentFormValue } from './models/student';
import { StudentService } from './services/student';

@Component({
  imports: [StudentGrid, StudentFormDialog],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  protected readonly studentService = inject(StudentService);

  protected readonly dialogOpen = signal(false);
  protected readonly editingStudent = signal<Student | null>(null);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    void this.studentService.loadAll();
  }

  openAddDialog(): void {
    this.editingStudent.set(null);
    this.formError.set(null);
    this.dialogOpen.set(true);
  }

  openEditDialog(student: Student): void {
    this.editingStudent.set(student);
    this.formError.set(null);
    this.dialogOpen.set(true);
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editingStudent.set(null);
  }

  async saveStudent(value: StudentFormValue): Promise<void> {
    const editing = this.editingStudent();

    try {
      if (editing) {
        await this.studentService.update(editing.id, value);
      } else {
        await this.studentService.create(value);
      }

      this.closeDialog();
    } catch (err) {
      this.formError.set(this.describeError(err));
    }
  }

  async removeStudent(student: Student): Promise<void> {
    if (!confirm(`Remove ${student.fullName}?`)) {
      return;
    }

    await this.studentService.remove(student.id);
  }

  private describeError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 409 && typeof err.error === 'string') {
        return err.error;
      }

      if (err.status === 400) {
        return 'Please check the highlighted fields and try again.';
      }
    }

    return 'Something went wrong while saving the student. Please try again.';
  }
}
