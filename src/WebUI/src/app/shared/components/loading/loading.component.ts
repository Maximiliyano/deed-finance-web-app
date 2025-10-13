import { ChangeDetectionStrategy, Component } from '@angular/core';
import { LoadingService } from '../../services/loading.service';

@Component({
    selector: 'app-loading',
    templateUrl: './loading.component.html',
    styleUrl: './loading.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class LoadingComponent {
  isLoading$ = this.loadingService.isLoading$;

  constructor(private loadingService: LoadingService) { }
}
