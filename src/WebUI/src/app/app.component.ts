import { Component } from '@angular/core';
import { AuthService } from './modules/auth/services/auth-service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: false
})
export class AppComponent {
    constructor(private readonly authService: AuthService) {
        authService.me().subscribe();
    }
}
