import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'app-footer',
    templateUrl: './footer.component.html',
    styleUrl: './footer.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FooterComponent {
}
