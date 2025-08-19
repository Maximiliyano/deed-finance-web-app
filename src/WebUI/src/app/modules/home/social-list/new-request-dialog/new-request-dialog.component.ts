import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { FormFields } from '../../../../shared/components/forms/models/form-fields';
import { SocialPackageEnum } from '../social-list.component';

@Component({
  selector: 'app-new-request-dialog',
  templateUrl: './new-request-dialog.component.html',
  styleUrl: './new-request-dialog.component.scss'
})
export class NewRequestDialogComponent implements OnInit {
  // constructor
  // @Inject(MAT_DIALOG_DATA) data: { name: ... } valueSubOptions
  value: string;
  valueSubOptions: string[]; // OPTION 1

  form: FormGroup;
  fields: FormFields[];

  ngOnInit(): void {
    this.form = new FormGroup({
      SubOptions: new FormControl(this.valueSubOptions), // depends on selected option show fields
      Subject: new FormControl(''),
      Calendar: new FormControl('')
    });

    this.fields = [];

    this.addSubOptions();

    // OPTION 1: I take value from parent component Expense, pass SubOptions Expenses
    // before initialize I check Expense - I add needed fields
    if (this.value as SocialPackageEnum === SocialPackageEnum.ExpenseCompensation) {
      this.fields.push({
        label: "Calendar",
        controlName: "Calendar",
        dateTimePicker: {}
      });
    }
    // TODO If you need for specific SocialPackageEnum fields, you just create a condition for it

  }

  addSubOptions(): void {
    if (this.valueSubOptions.length > 0 && !this.fields.find(x => x.label === "SubOptions")) {
      this.fields.push({
        label: 'SubOptions',
        controlName: 'SubOptions',
        select: {
          onChange: (value) => this.handleSubOptionClick(value),
          options: this.valueSubOptions.map(x => { return { key: x, value: x }}) }
      });
    };
  }

  handleSubOptionClick(event: Event): void {
    const subOptionValue = (event.target as HTMLSelectElement).value;

    switch (subOptionValue) {
      case "Team Lunch":
        this.form = new FormGroup({
          SubOptions: new FormControl(this.valueSubOptions),
          Subject: new FormControl(''),
          Radio: new FormControl(false),
          Employees: new FormControl([]),
          Projects: new FormControl([])
        });
        this.fields.push(
          {
            label: 'Subject',
            controlName: "Subject",
            input: {
              type: 'text'
            }
          });

        this.fields.push(
          {
            label: 'Radio',
            controlName: 'Radio',
            input: {
              type: 'number'
            }
          }
        );

        this.fields.push({
          label: 'Employees',
          controlName: 'Employees',
          select:{
            options: []
          }
        })

        this.fields.push({
          label: 'Projects',
          controlName: 'Projects',
          select:{
            options: []
          }
        })
        break;

      case "Certifaction":
        this.form = new FormGroup({
          SubOptions: new FormControl(this.valueSubOptions),
          Name: new FormControl('')
        });

        this.fields = [
          {
            label: 'Name',
            controlName: 'Name',
            input: {
              type: 'text'
            }
          }
        ];

        this.addSubOptions();
        break;
    }

  }
}
