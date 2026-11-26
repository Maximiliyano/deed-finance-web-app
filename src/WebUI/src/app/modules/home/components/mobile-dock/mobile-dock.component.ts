import { ChangeDetectionStrategy, Component, DestroyRef, NgZone, OnInit, inject, signal } from '@angular/core';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { DashboardStore } from '../../services/dashboard-store.service';
import { QuickAddService } from '../../services/quick-add.service';
import { QuickAction, QuickActionSheetComponent } from '../quick-action-sheet/quick-action-sheet.component';

interface DockTab {
  id: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-mobile-dock',
  templateUrl: './mobile-dock.component.html',
  styleUrl: './mobile-dock.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MobileDockComponent implements OnInit {
  private readonly store = inject(DashboardStore);
  private readonly quickAdd = inject(QuickAddService);
  private readonly dialogService = inject(DialogService);
  private readonly zone = inject(NgZone);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly leftTabs: DockTab[] = [
    { id: 'kpi', label: 'Home', icon: 'fa-house' },
    { id: 'capitals', label: 'Capitals', icon: 'fa-wallet' }
  ];
  protected readonly rightTabs: DockTab[] = [
    { id: 'expenses', label: 'Activity', icon: 'fa-receipt' },
    { id: 'utilityBills', label: 'Plans', icon: 'fa-calendar-check' }
  ];

  protected readonly active = signal('kpi');

  private frame = 0;

  ngOnInit(): void {
    this.zone.runOutsideAngular(() => window.addEventListener('scroll', this.onScroll, { passive: true }));
    this.destroyRef.onDestroy(() => {
      window.removeEventListener('scroll', this.onScroll);
      cancelAnimationFrame(this.frame);
    });
  }

  protected go(id: string): void {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  protected openSheet(): void {
    const ref = this.dialogService.open(QuickActionSheetComponent, {
      data: { canTransfer: !this.store.isAnonymous() }
    });

    ref.afterClosed$.subscribe((action: QuickAction | null) => {
      if (action === 'expense') void this.quickAdd.addTransaction('expense');
      else if (action === 'income') void this.quickAdd.addTransaction('income');
      else if (action === 'transfer') void this.quickAdd.transfer();
    });
  }

  private readonly onScroll = (): void => {
    cancelAnimationFrame(this.frame);
    this.frame = requestAnimationFrame(() => {
      const probe = window.innerHeight * 0.4;
      const ids = [...this.leftTabs, ...this.rightTabs].map(tab => tab.id);
      let current = ids[0];

      for (const id of ids) {
        const top = document.getElementById(id)?.getBoundingClientRect().top;
        if (top !== undefined && top <= probe) current = id;
      }

      if (current !== this.active()) {
        this.zone.run(() => this.active.set(current));
      }
    });
  };
}
