import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-error',
    templateUrl: './error.component.html',
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorComponent implements OnInit {
    readonly statusCode = 404;
    readonly message = 'The requested page could not be found.';

    ngOnInit() {
        document.title = `Deed - ${this.statusCode} error`;
    }
}
