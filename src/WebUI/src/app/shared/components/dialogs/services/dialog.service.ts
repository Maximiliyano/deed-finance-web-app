import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable, Injector, Type, inject } from '@angular/core';
import { DialogConfig } from '../models/dialog-config';
import { DialogRef } from '../models/dialog-ref';
import { DIALOG_DATA } from '../models/dialog-consts';
import { DialogComponent } from '../dialog.component';
import { ScrollLockService } from '../../../../core/services/scroll-lock.service';

const LEAVE_MS = 200;

@Injectable({ providedIn: 'root' })
export class DialogService {
  private readonly appRef = inject(ApplicationRef);
  private readonly injector = inject(EnvironmentInjector);
  private readonly scrollLock = inject(ScrollLockService);

  private openDialogs: ComponentRef<DialogComponent>[] = [];

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
    containerRef.instance.width = config?.width ?? null;
    containerRef.instance.childComponentType = component;
    containerRef.instance.childInjector = childInjector;

    this.appRef.attachView(containerRef.hostView);
    document.body.appendChild(containerRef.location.nativeElement);
    
    this.openDialogs.push(containerRef);
    this.scrollLock.lock();

    const sub = dialogRef.afterClosed$.subscribe({
      next: () => {
        sub.unsubscribe();
        this.close(containerRef);
      }
    });

    return dialogRef;
  }

  private close(ref: ComponentRef<DialogComponent>) {
    this.scrollLock.unlock();

    this.openDialogs = this.openDialogs.filter(x => x !== ref);
    this.openDialogs.forEach((d, i) => d.instance.index = i);

    const destroy = () => {
      this.appRef.detachView(ref.hostView);
      ref.destroy();
    };

    if (ref.instance.closing || window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      destroy();
      return;
    }

    ref.location.nativeElement.classList.add('is-leaving');
    setTimeout(destroy, LEAVE_MS);
  }
}
