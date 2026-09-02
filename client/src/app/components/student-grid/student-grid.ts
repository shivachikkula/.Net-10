import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Student } from '../../models/student';

@Component({
  imports: [],
  selector: 'app-student-grid',
  styleUrl: './student-grid.css',
  templateUrl: './student-grid.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentGrid {
  readonly students = input<Student[]>([]);
  readonly loading = input(false);

  readonly edit = output<Student>();
  readonly remove = output<Student>();
}
