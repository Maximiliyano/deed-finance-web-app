import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { UtilityBillResponse } from '../../models/utility-bill';

export type UtilityBillVariant = 'pending' | 'paid' | 'inactive';

@Component({
  selector: 'app-utility-bill-list',
  templateUrl: './utility-bill-list.component.html',
  styleUrl: './utility-bill-list.component.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UtilityBillListComponent {
  bills = input.required<UtilityBillResponse[]>();
  variant = input.required<UtilityBillVariant>();
  title = input.required<string>();

  pay = output<UtilityBillResponse>();
  edit = output<UtilityBillResponse>();
  remove = output<UtilityBillResponse>();
}
