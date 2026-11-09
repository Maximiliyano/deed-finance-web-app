import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-panel-state',
  templateUrl: './panel-state.component.html',
  styleUrl: './panel-state.component.scss',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PanelStateComponent {
  loading = input(false);
  empty = input(false);
  icon = input('fa-circle-info');
  title = input('');
  hint = input('');
}
