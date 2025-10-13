import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormGroup,  } from '@angular/forms';
import { FormField } from './models/form-field';
import { FormButton } from './models/form-button';
import { Subject, takeUntil } from 'rxjs';

@Component({
    selector: 'app-form',
    templateUrl: './form.component.html',
    standalone: false
})
export class FormComponent implements OnInit, OnDestroy {
  @Input() headerText: string = 'Default form';
  @Input() form: FormGroup = new FormGroup({});
  @Input() fields: FormField[] = [];
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

  private $unsubscribe = new Subject<void>;

  ngOnInit(): void {
    this.form.statusChanges
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => this.updateSubmitButtonState()
      });
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
