import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrl: './layout.component.scss',
    standalone: false
})
export class LayoutComponent {
  @Input() title: string = "Template Title";
}
