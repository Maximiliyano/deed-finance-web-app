import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-icon',
    templateUrl: './icon.component.html',
    standalone: false
})
export class IconComponent {
  @Input() name: string = 'fa-icon';
}
