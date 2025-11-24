import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable, Injector, Type } from '@angular/core';
import { DialogConfig } from '../models/dialog-config';
import { DialogRef } from '../models/dialog-ref';
import { DIALOG_DATA } from '../models/dialog-consts';
import { DialogComponent } from '../dialog.component';

@Injectable({ providedIn: 'root' })
export class DialogService {
  private openDialogs: ComponentRef<DialogComponent>[] = [];

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector
  ) {}

  open<T, R = any>(
    component: Type<T>,
    config?: DialogConfig<any>
  ): DialogRef<R> {
    const dialogRef = new DialogRef<R>();

    const childInjector = Injector.create({
      parent: this.injector,
      providers: [
        { provide: DialogRef, useValue: dialogRef },
        { provide: DIALOG_DATA, useValue: config?.data }
      ]
    });

    const containerRef = createComponent(DialogComponent, {
      environmentInjector: this.injector
    });

    containerRef.instance.index = this.openDialogs.length;
    containerRef.instance.childComponentType = component;
    containerRef.instance.childInjector = childInjector;

    this.appRef.attachView(containerRef.hostView);
    document.body.appendChild(containerRef.location.nativeElement);
    
    this.openDialogs.push(containerRef);

    const sub = dialogRef.afterClosed$.subscribe({
      next: () => {
        sub.unsubscribe();
        this.close(containerRef);
      }
    });

    return dialogRef;
  }

  private close(ref: ComponentRef<DialogComponent>) {
    this.appRef.detachView(ref.hostView);
    ref.destroy();

    this.openDialogs = this.openDialogs.filter(x => x !== ref);
    this.openDialogs.forEach((d, i) => d.instance.index = i);
  }

  closeAll() {
    this.openDialogs.forEach(ref => {
      this.appRef.detachView(ref.hostView);
      ref.destroy();
    });
    this.openDialogs = [];
  }
}
