import { Component, OnInit, OnDestroy, ViewChild, ViewContainerRef, ComponentRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { DialogData, DialogService } from '../../services/dialog.service';

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrl: './dialog.component.scss',
})
export class DialogComponent implements OnInit, OnDestroy {
  visible = false;
  dialogData: DialogData | null = null;

  private sub!: Subscription;

  @ViewChild('dialogContainer', { read: ViewContainerRef, static: false }) container!: ViewContainerRef;
  private componentRef?: ComponentRef<any>;

  constructor(private dialogService: DialogService) {}

  ngOnInit() {
    this.sub = this.dialogService.dialogState$.subscribe(dialogState => {
      this.dialogData = dialogState;
      this.visible = !!dialogState;

      if (this.visible) {
        setTimeout(() => this.loadComponent(), 0);
      } else {
        this.clearContainer();
      }
    });
  }

  loadComponent() {
    if (!this.container) return;

    this.container.clear();

    if (this.dialogData) {
      this.componentRef = this.container.createComponent(this.dialogData.component);

      if (this.dialogData.data) {
        Object.assign(this.componentRef.instance, this.dialogData.data);
      }

      const instance = this.componentRef.instance as any;

      if (instance.submitted && typeof instance.submitted.subscribe === 'function' && this.dialogData.onSubmit) {
        instance.submitted.subscribe((result: any) => {
          this.dialogData?.onSubmit?.(result);
        });
      }
    }
  }

  clearContainer() {
    if (this.container) {
      this.container.clear();
    }
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  close() {
    this.dialogService.close();
  }
}
