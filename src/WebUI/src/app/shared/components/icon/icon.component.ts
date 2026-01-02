import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-icon',
    templateUrl: './icon.component.html',
    standalone: true
})
export class IconComponent {
  @Input({ required: true }) name: string = 'fa-icon';
}
