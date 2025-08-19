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

    // OPTION 1: pass options Expenses -> Education,Certification
    let valueSubOptions: string[] = [];
    switch (selectedValue as SocialPackageEnum) {
      case SocialPackageEnum.ExpenseCompensation:
        valueSubOptions = [ "Education", "Certifaction", "Team Lunch" ];
        break;
    }
    //===========================================================

    this.dialogService.open({
      component: NewRequestDialogComponent,
      data: {
        value: selectedValue,
        valueSubOptions: valueSubOptions
      }
    })
  }
}

export enum SocialPackageEnum {
  Vacation = "Vacation",
  ExpenseCompensation = "ExpenseCompensation"
};

export const SOCIAL_LIST: Social[] = [
  {name: SocialPackageEnum.Vacation, property: 'vacation', disabled: false},
  {name: SocialPackageEnum.ExpenseCompensation, property: 'expense-compensation', disabled: false}
];

type Social = {
  name: string;
  property: string;
  disabled: boolean;
}
