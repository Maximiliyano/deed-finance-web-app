import { Component, OnInit, OnDestroy, ViewContainerRef, ComponentRef, QueryList, ViewChildren } from '@angular/core';
import { Subscription } from 'rxjs';
import { DialogData, DialogService } from '../../services/dialog.service';

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrl: './dialog.component.scss',
})
export class DialogComponent implements OnInit, OnDestroy {
  dialogStack: DialogData[] = [];
  private sub!: Subscription;

  @ViewChildren('dialogContainer', { read: ViewContainerRef })
  containers!: QueryList<ViewContainerRef>;

  private componentRefs: ComponentRef<any>[] = [];

  constructor(private dialogService: DialogService) {}

  ngOnInit() {
    this.sub = this.dialogService.dialogStack$.subscribe(stack => {
      this.dialogStack = stack;
      setTimeout(() => this.renderDialogs(), 0);
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
    this.componentRefs.forEach(ref => ref.destroy());
  }

  private renderDialogs() {
    if (!this.containers || this.containers?.length === 0) return;

    this.componentRefs.forEach(ref => ref.destroy());
    this.componentRefs = [];

    this.dialogStack.forEach((dialog, index) => {
      const container = this.containers.toArray()[index];
      container.clear();

      const componentRef = container.createComponent(dialog.component);

      if (dialog.data) {
        Object.assign(componentRef.instance, dialog.data);
      }

      const instance = componentRef.instance as any;
      if (instance.submitted && typeof instance.submitted.subscribe === 'function' && dialog.onSubmit) {
        instance.submitted.subscribe({
          next: (result: any | null) => dialog?.onSubmit?.(result)
        });
      }

      this.componentRefs.push(componentRef);
    });
  }

  close() {
    this.dialogService.close();
  }
}
