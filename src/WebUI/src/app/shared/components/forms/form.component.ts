import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule,  } from '@angular/forms';
import { FormField } from './models/form-field';
import { FormButton } from './models/form-button';
import { Subject, takeUntil } from 'rxjs';
import { FormErrorComponent } from "./form-error/form-error.component";
import { DatePickerComponent } from '../date-picker/date-picker.component';
import { SelectiveInputComponent } from "../inputs/selective-input.component/selective-input.component";

@Component({
    selector: 'app-form',
    templateUrl: './form.component.html',
    imports: [ReactiveFormsModule, FormErrorComponent, DatePickerComponent, SelectiveInputComponent],
    standalone: true
})
export class FormComponent implements OnInit, OnDestroy {
  @Input() headerText: string = 'Default form';
  @Input({ required: true }) form: FormGroup = new FormGroup({});
  @Input({ required: true }) fields: FormField[] = [];
  @Input() buttons: FormButton[] = [
    {
      type: 'submit',
      text: 'Save',
      styles: `bg-blue-600 text-white hover:bg-blue-700`
    },
    {
      type: 'button',
      text: 'Cancel',
      styles: 'bg-gray-300 hover:bg-gray-400',
      onClick: () => this.onCancel()
    }
  ];

  @Output() submitForm = new EventEmitter<void>();
  @Output() cancelForm = new EventEmitter<void>();
  
  private $unsubscribe = new Subject<void>();

  formControl(controlName: string): AbstractControl<any, any, any> | null {
    return this.form.get(controlName);
  }
  
  ngOnInit(): void {
    this.form.statusChanges
    .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => this.updateSubmitButtonState()
      });
    this.updateSubmitButtonState();
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
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
