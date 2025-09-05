import { Component } from '@angular/core';

@Component({
  selector: 'app-date-tab-picker',
  templateUrl: './date-tab-picker.html',
  styleUrl: './date-tab-picker.scss'
})
export class DateTabPicker {
  tabs = [
    'Day',
    'Week',
    'Month',
    'Year',
    'Custom'
  ];
}
