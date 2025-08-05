import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup,  } from '@angular/forms';
import { FormFields } from './models/form-fields';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html'
})
export class FormComponent {
  @Input() form: FormGroup = new FormGroup({});
  @Input() headerText: string = 'Default form';
  @Input() submitButtonText: string = 'Submit';
  @Input() cancelButtonText: string = 'Cancel';
  @Input() fields: FormFields[] = [];

  @Output() submitForm = new EventEmitter<void>();
  @Output() cancelForm = new EventEmitter<void>();

  onSubmit() {
    this.submitForm.emit();
  }

  onCancel() {
    this.cancelForm.emit();
  }
}
