import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FormFields } from '../../../../shared/components/forms/models/form-fields';
import { optionMega, SocialPackageEnum, SocialPackageEnumOption } from '../social-list.component';
import { DialogService } from '../../../../shared/services/dialog.service';

@Component({
  selector: 'app-new-request-dialog',
  templateUrl: './new-request-dialog.component.html',
  styleUrl: './new-request-dialog.component.scss'
})
export class NewRequestDialogComponent implements OnInit {
  // You receive the optionMega from inject, matdialogdata constructor
  constructor(
    private readonly dialogService: DialogService,
    // @Inject(MAT_DIALOG_DATA) data: { name: ... }
  ) {
  }
  optionMega: optionMega;

  form: FormGroup;
  fields: FormFields[];

  isUserManager: boolean = true;

  ngOnInit(): void {
    // OPTION 2: I init form & fields based on OptionMega.InputValue
    this.initForm();

    // If you need for specific SocialPackageEnum fields, you just create a condition for it
    this.initFields();
  }

  initForm(): void {
    switch(this.optionMega.name as SocialPackageEnum) {
      case SocialPackageEnum.ExpenseCompensation: {
        this.form = new FormGroup({
          SubOption: new FormControl('', [ Validators.required ]), // depends on selected option show fields
          Calendar: new FormControl('')
        });

        if (this.isUserManager) {
          this.form.addControl('Subject', new FormControl(''));
        }
        break;
      }
      case SocialPackageEnum.Vacation: {
        this.form = new FormGroup({
          Date: new FormControl(''),
          Purpose: new FormControl('')
        })
      }
    }
  }

  initFields(): void {
    // init parent enum options fields
    switch(this.optionMega.name as SocialPackageEnum) {
      case SocialPackageEnum.ExpenseCompensation: {
        this.fields = [
          {
            label: 'Subsection options',
            controlName: 'SubOption',
            select: {
              onChange: (value) => this.handleSubOptionClick(value),
              options: this.optionMega.subOptions.map(x => { return { key: x, value: x }})
            }
          },
          {
            label: 'Calendar',
            controlName: 'Calendar',
            dateTimePicker: {}
          }
        ];

        if (this.isUserManager) {
          this.fields.push({
            label: 'Subject',
            controlName: 'Subject',
            input: {
              type: 'text'
            }
          });
        }

        break;
      }
      case SocialPackageEnum.Vacation: {
        this.fields = [
          {
            label: 'Date Time',
            controlName: 'Date',
            dateTimePicker: {}
          },
          {
            label: 'Purpose',
            controlName: 'Purpose',
            input: {
              type: 'text'
            }
          }
        ]
      }
    }
  }

  initSubOptions(subOption: string): void {
    switch (subOption) {
      case SocialPackageEnumOption.Education: {
        // add/update controls you could extract it into const of EducationSubOptions
        this.setFormControls(['Knowledge', 'Degree']);

        // update form fields you can you constants instead of the methods
        this.fields = this.getEducationFields();
        break;
      }

      case SocialPackageEnumOption.TeamLunch: {
        this.setFormControls(['SubOption', 'Subject', 'Eaten', 'Employees', 'Projects']);
        this.fields = this.getTeamFields();
        break;
      }

      case SocialPackageEnumOption.Certifaction: {
        this.setFormControls(['SubOption', 'Name']);
        this.fields = this.getCertificateFields();
        break;
      }

      default: {
        // if selected invalid suboptions, do nothing or push an error for ex

        // cleanup formControlls instead of recreationg new FormControl...
        Object.keys(this.form.controls).forEach(ctrl => this.form.removeControl(ctrl));
        // cleanup fields on the form
        this.fields = [];
        break;
      }
    }
  }

  // If you want not only to create FormControl & add value just pass not string[] -> SubOptionProps - (controlName, value)
  setFormControls(subOptions: string[]): void {
    subOptions.forEach(ctrl => {
      if (!this.form.contains(ctrl)) {
        this.form.addControl(ctrl, new FormControl(''));
      }
    })
  }

  handleSubOptionClick(event: Event): void {
    const subOptionValue = (event.target as HTMLSelectElement).value;

    this.initSubOptions(subOptionValue);
  }

  handleClose(): void {
    this.dialogService.close();
  }

  getCertificateFields(): FormFields[]  {
    return [
      {
        label: 'Subsection options',
        controlName: 'SubOption',
        select: {
          onChange: (value) => this.handleSubOptionClick(value),
          options: this.optionMega.subOptions.map(x => { return { key: x, value: x }})
        }
      },
      {
        label: 'Name',
        controlName: 'Name',
        input: { type: 'text' }
      }
    ];
  }

  getEducationFields(): FormFields[] {
    return [
      {
        label: 'Knowledge',
        controlName: 'Knowledge',
        input: { type: 'text' }
      },
      {
        label: `Degree's`,
        controlName: 'Degree',
        select: {
          options: [
            { key: "Select a degree...", value: ''},
            { key: "Minecraft", value: "Minecraft" }
          ]
        }
      }
    ];
  }

  getTeamFields(): FormFields[] {
    return [
      {
        label: 'Subsection options',
        controlName: 'SubOption',
        select: {
          onChange: (value) => this.handleSubOptionClick(value),
          options: this.optionMega.subOptions.map(x => { return { key: x, value: x }})
        }
      },
      {
        label: 'Subject',
        controlName: "Subject",
        input: {
          type: 'text'
        }
      },
      {
        label: 'Did you eaten your Bo-bo?',
        controlName: 'Eaten',
        input: {
          type: 'checkbox'
        }
      },
      {
        label: 'Employees',
        controlName: 'Employees',
        select:{
          options: [ { key: 'Select a employee...', value: '' } ]
        }
      },
      {
        label: 'Projects',
        controlName: 'Projects',
        select:{
          options: [ { key: 'Select a project...', value: '' } ]
        }
      }
    ];
  }
}
