import { Component, ViewContainerRef, Type, Injector, AfterViewInit, ViewChild, Input } from '@angular/core';
import { DialogRef } from './models/dialog-ref';

@Component({
    selector: 'app-dialog',
    templateUrl: './dialog.component.html',
    styleUrl: './dialog.component.scss',
    standalone: true
})
export class DialogComponent implements AfterViewInit {
  @ViewChild('dialogContainer', { read: ViewContainerRef })
  container!: ViewContainerRef;
  
  @Input() index = 0;
  @Input() childComponentType!: Type<any>;
  @Input() childInjector!: Injector;

  ngAfterViewInit(): void {
    this.container.createComponent(this.childComponentType, {
      injector: this.childInjector
    });
  }

  close(): void {
    const ref = this.childInjector.get(DialogRef, null);
    ref?.close();
  }
}
