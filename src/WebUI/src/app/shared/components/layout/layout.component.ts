import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrl: './layout.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent {
  @Input() text: string = "Template Title";
}
