import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-icon',
    templateUrl: './icon.component.html',
    standalone: false
})
export class IconComponent {
  @Input({ required: true }) name: string = 'fa-icon';
}
