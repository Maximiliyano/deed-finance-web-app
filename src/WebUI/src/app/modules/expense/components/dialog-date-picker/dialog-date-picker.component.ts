import {Component, EventEmitter, Output} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";

@Component({
  selector: 'app-dialog-date-picker',
  templateUrl: './dialog-date-picker.component.html',
  styleUrl: './dialog-date-picker.component.scss'
})
export class DialogDatePickerComponent {
  @Output() submitted = new EventEmitter<{}>();

  startDate: Date | null;
  endDate: Date | null;
  allTime: boolean;

  datePickerForm: FormGroup;

  constructor(private fb: FormBuilder)
  {
    this.datePickerForm = this.fb.group({
      startDate: [this.startDate, Validators.required],
      endDate: [this.endDate, Validators.required],
      allTimeCheck: [this.allTime]
    });

    this.toggleAllTime();
  }

  onSubmit(): void {
    if (!this.datePickerForm.valid) {
      return;
    }

    const { startDate, endDate, allTimeCheck } = this.datePickerForm.value;

    this.submitted.next({
      startDate: startDate,
      endDate: endDate,
      allTime: allTimeCheck
    });
  }

  cancel(): void {
    this.submitted.next({
      startDate: this.startDate,
      endDate: this.endDate,
      allTime: this.allTime,
    });
  }

  toggleAllTime(): void {
    this.datePickerForm.get('allTimeCheck')?.valueChanges
      .subscribe({
        next: (value: boolean) => {
          if (value) {
            this.datePickerForm.get('startDate')?.disable();
            this.datePickerForm.get('endDate')?.disable();
          } else {
            this.datePickerForm.get('startDate')?.enable();
            this.datePickerForm.get('endDate')?.enable();
          }
        }
    });
  }
}
