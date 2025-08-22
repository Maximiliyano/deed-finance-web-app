import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup,  } from '@angular/forms';
import { FormField } from './models/form-field';
import { FormButton } from './models/form-button';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html'
})
export class FormComponent {
  @Input() headerText: string = 'Default form';
  @Input() form: FormGroup = new FormGroup({});
  @Input() fields: FormField[] = [];
  @Input() buttons: FormButton[] = [
    {
      type: 'submit',
      text: 'Save',
      styles: 'bg-blue-600 hover:bg-blue-700 text-white',
      disabled: this.form.invalid,
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

  onSubmit() {
    this.submitForm.emit();
  }

  onCancel() {
    this.cancelForm.emit();
  }
}
