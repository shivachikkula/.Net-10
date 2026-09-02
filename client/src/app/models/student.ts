export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  dateOfBirth: string;
  department: string | null;
  gpa: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface StudentFormValue {
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: string;
  department: string | null;
  gpa: number;
}
