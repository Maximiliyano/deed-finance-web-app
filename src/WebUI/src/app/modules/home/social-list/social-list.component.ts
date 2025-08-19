import { Component, Inject, OnInit } from '@angular/core';
import { DialogService } from '../../../shared/services/dialog.service';
import { NewRequestDialogComponent } from './new-request-dialog/new-request-dialog.component';

@Component({
  selector: 'app-social-list',
  templateUrl: './social-list.component.html',
  styleUrl: './social-list.component.scss'
})
export class SocialListComponent implements OnInit {
  options: Social[];

  constructor( //private dialog: MatDialog
    private readonly dialogService: DialogService) {}

  ngOnInit(): void {
    this.options = SOCIAL_LIST;
  }

  handleClick(event: Event): void { // button - (click)="handleClick"
    const selectedValue = (event.target as HTMLSelectElement).value; // social_list value e.g. Vacation...

    // this.dialog.open(NewRequestDialogComponent, { value: selectedValue })

    // OPTION 2: pass options Expenses with SubOptions
    let optionMegaTron: optionMega[] = [
      {
        name: SocialPackageEnum.ExpenseCompensation,
        subOptions: Object.values(SocialPackageEnumOption)
      },
      {
        name: SocialPackageEnum.Vacation,
        subOptions: []
      }
    ]

    let optionMega = optionMegaTron.find(x => x.name == selectedValue as SocialPackageEnum)!;

    if (optionMega) {
      this.dialogService.open({
        component: NewRequestDialogComponent,
        data: {
          optionMega: optionMega
        }
      })
    }
  }
}

export type optionMega = {
  name: string;
  subOptions: string[]
};

export enum SocialPackageEnum {
  Vacation = "Vacation",
  ExpenseCompensation = "ExpenseCompensation"
};

export enum SocialPackageEnumOption {
  Education = "Education",
  Certifaction = "Certifaction",
  TeamLunch = "Team Lunch"
}

export const SOCIAL_LIST: Social[] = [
  {name: SocialPackageEnum.Vacation, property: 'vacation', disabled: false},
  {name: SocialPackageEnum.ExpenseCompensation, property: 'expense-compensation', disabled: false}
];

type Social = {
  name: string;
  property: string;
  disabled: boolean;
}
