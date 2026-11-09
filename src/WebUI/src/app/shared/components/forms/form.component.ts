import { Component, EventEmitter, Input, OnInit, Output, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormGroup, ReactiveFormsModule,  } from '@angular/forms';
import { FormField } from './models/form-field';
import { FormButton } from './models/form-button';

import { FormErrorComponent } from "./form-error/form-error.component";
import { DatePickerComponent } from '../date-picker/date-picker.component';
import { SelectiveInputComponent } from "../inputs/selective-input.component/selective-input.component";

@Component({
    selector: 'app-form',
    templateUrl: './form.component.html',
    styleUrl: './form.component.scss',
    imports: [ReactiveFormsModule, FormErrorComponent, DatePickerComponent, SelectiveInputComponent],
    standalone: true
})
export class FormComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  @Input() headerText: string = 'Default form';
  @Input() headerIcon: string = '';
  @Input({ required: true }) form: FormGroup = new FormGroup({});
  @Input({ required: true }) fields: FormField[] = [];
  @Input() buttons: FormButton[] = [
    {
      type: 'submit',
      text: 'Save',
      styles: 'btn-primary'
    },
    {
      type: 'button',
      text: 'Cancel',
      styles: 'btn-secondary',
      onClick: () => this.onCancel()
    }
  ];

  @Output() submitForm = new EventEmitter<void>();
  @Output() cancelForm = new EventEmitter<void>();
  
  formControl(controlName: string): AbstractControl<any, any, any> | null {
    return this.form.get(controlName);
  }
  
  ngOnInit(): void {
    this.form.statusChanges
    .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.updateSubmitButtonState()
      });
    this.updateSubmitButtonState();
  }

  onSubmit() {
    this.submitForm.emit();
  }

  onCancel() {
    this.cancelForm.emit();
  }

  private updateSubmitButtonState(): void {
    const submitButton = this.buttons.find(b => b.type === 'submit');
    if (submitButton) {
      submitButton.disabled = this.form.invalid;
    }
  }
}
