import { HttpClient } from '@angular/common/http';
import { Service, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Student, StudentFormValue } from '../models/student';

@Service()
export class StudentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/students`;

  readonly students = signal<Student[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  async loadAll(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const students = await firstValueFrom(this.http.get<Student[]>(this.baseUrl));
      this.students.set(students);
    } catch {
      this.error.set('Could not load students. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }

  async create(value: StudentFormValue): Promise<void> {
    const created = await firstValueFrom(this.http.post<Student>(this.baseUrl, value));
    this.students.update((students) => [...students, created]);
  }

  async update(id: number, value: StudentFormValue): Promise<void> {
    const updated = await firstValueFrom(this.http.put<Student>(`${this.baseUrl}/${id}`, value));
    this.students.update((students) => students.map((s) => (s.id === id ? updated : s)));
  }

  async remove(id: number): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${this.baseUrl}/${id}`));
    this.students.update((students) => students.filter((s) => s.id !== id));
  }
}
