import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StudentGrid } from './student-grid';

describe('StudentGrid', () => {
  let component: StudentGrid;
  let fixture: ComponentFixture<StudentGrid>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentGrid],
    }).compileComponents();

    fixture = TestBed.createComponent(StudentGrid);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
