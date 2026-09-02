import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { StudentFormValue } from '../models/student';
import { StudentService } from './student';

const baseUrl = `${environment.apiBaseUrl}/students`;

const studentValue: StudentFormValue = {
  firstName: 'Ada',
  lastName: 'Lovelace',
  email: 'ada@example.com',
  dateOfBirth: '2000-12-10',
  department: 'Mathematics',
  gpa: 3.9,
};

describe('StudentService', () => {
  let service: StudentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(StudentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('loadAll populates students on success', async () => {
    const loadPromise = service.loadAll();

    httpMock.expectOne(baseUrl).flush([{ ...studentValue, id: 1, fullName: 'Ada Lovelace', createdAtUtc: '', updatedAtUtc: null }]);
    await loadPromise;

    expect(service.students().length).toBe(1);
    expect(service.error()).toBeNull();
  });

  it('loadAll sets an error message on failure', async () => {
    const loadPromise = service.loadAll();

    httpMock.expectOne(baseUrl).flush('failure', { status: 500, statusText: 'Server Error' });
    await loadPromise;

    expect(service.error()).not.toBeNull();
  });

  it('create appends the new student to the list', async () => {
    const createPromise = service.create(studentValue);

    httpMock
      .expectOne(baseUrl)
      .flush({ ...studentValue, id: 2, fullName: 'Ada Lovelace', createdAtUtc: '', updatedAtUtc: null });
    await createPromise;

    expect(service.students().some((s) => s.id === 2)).toBe(true);
  });

  it('remove drops the student from the list', async () => {
    const createPromise = service.create(studentValue);
    httpMock
      .expectOne(baseUrl)
      .flush({ ...studentValue, id: 3, fullName: 'Ada Lovelace', createdAtUtc: '', updatedAtUtc: null });
    await createPromise;

    const removePromise = service.remove(3);
    httpMock.expectOne(`${baseUrl}/3`).flush(null);
    await removePromise;

    expect(service.students().some((s) => s.id === 3)).toBe(false);
  });
});
