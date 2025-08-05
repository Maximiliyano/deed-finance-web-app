import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-form-error',
  templateUrl: './form-error.component.html'
})
export class FormErrorComponent {
  @Input() form: FormGroup;
  @Input() controlName: string;

  get errorMessage(): string | null {
    const control = this.form?.get(this.controlName);
    if (!control || !control.errors || !control.touched) return null;

    if (control.errors['required']) return 'This field is required.';
    if (control.errors['minlength']) return `Minimum length is ${control.errors['minlength'].requiredLength}.`;
    if (control.errors['maxlength']) return `Maximum length is ${control.errors['maxlength'].requiredLength}.`;
    if (control.errors['min']) return `Value must be at least ${control.errors['min'].min}.`;
    if (control.errors['max']) return `Value must be no more than ${control.errors['max'].max}.`;

    return null;
  }
}
