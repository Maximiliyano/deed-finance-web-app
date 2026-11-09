import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Injectable, inject } from '@angular/core';
import { OverlayComponent } from '../components/overlay/overlay.component';

@Injectable({
  providedIn: 'root'
})
export class OverlayService {
  private readonly overlay = inject(Overlay);

  create(): OverlayRef {
    const overlayRef = this.overlay.create();

    const portal = new ComponentPortal(OverlayComponent);

    overlayRef.attach(portal);

    return overlayRef;
  }

  close(overlayRef: OverlayRef): void {
    overlayRef.detach();
  }
}