import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ErrorService } from '../../../shared/services/error.service';

@Component({
    selector: 'app-error',
    templateUrl: './error.component.html',
    standalone: true
})
export class ErrorComponent implements OnInit {
    statusCode = 500;
    message = 'Unexpected error occurred.';
    previousUrl = '/';

    constructor(
        private readonly errorService: ErrorService,
        private readonly route: ActivatedRoute) {}

    ngOnInit() {
        const error = this.errorService.getError();
        const routeStatus = this.route.snapshot.data['status'];

        if (error) {
            this.statusCode = error.status ?? 500;
            this.message = error.message ?? 'Unexpected error';
            this.previousUrl = this.errorService.getPreviousUrl() ?? '/';
            this.errorService.clear();
        } else if (routeStatus) {
            this.statusCode = routeStatus;
            this.message = 'The requested page could not be found.';
        } else {
            this.statusCode = 404;
            this.message = 'The requested page could not be found.';
        }

        document.title = `Deed - ${this.statusCode} error`;
    }
}
