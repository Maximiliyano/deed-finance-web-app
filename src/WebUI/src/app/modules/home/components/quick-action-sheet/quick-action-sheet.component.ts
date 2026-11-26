import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';

export type QuickAction = 'expense' | 'income' | 'transfer';

@Component({
  selector: 'app-quick-action-sheet',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-header">
      <h2 class="dialog-title">Add</h2>
    </div>
    <div class="sheet-actions">
      <button type="button" class="sheet-action sheet-action--expense" (click)="pick('expense')">
        <i class="fas fa-arrow-trend-down"></i>
        <span class="sheet-action-text">
          <strong>Expense</strong>
          <small>Money you spent</small>
        </span>
      </button>
      <button type="button" class="sheet-action sheet-action--income" (click)="pick('income')">
        <i class="fas fa-arrow-trend-up"></i>
        <span class="sheet-action-text">
          <strong>Income</strong>
          <small>Money you received</small>
        </span>
      </button>
      @if (data.canTransfer) {
        <button type="button" class="sheet-action sheet-action--transfer" (click)="pick('transfer')">
          <i class="fas fa-right-left"></i>
          <span class="sheet-action-text">
            <strong>Transfer</strong>
            <small>Move money between capitals</small>
          </span>
        </button>
      }
    </div>
  `,
  styles: `
    .sheet-actions {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      padding: var(--space-4) var(--space-5) var(--space-5);
    }

    .sheet-action {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      min-height: 3.5rem;
      padding: var(--space-3) var(--space-4);
      border: 1px solid var(--border-2);
      border-radius: var(--radius);
      background: var(--surface-2);
      color: var(--text);
      text-align: left;
      cursor: pointer;
      transition:
        background var(--t),
        border-color var(--t);

      &:hover,
      &:active {
        background: var(--surface);
        border-color: var(--action, var(--primary));
      }

      i {
        flex-shrink: 0;
        width: 1.5rem;
        font-size: var(--fs-xl);
        text-align: center;
        color: var(--action, var(--primary));
      }
    }

    .sheet-action-text {
      display: flex;
      flex-direction: column;

      strong {
        font-size: var(--fs-lg);
        font-weight: 600;
      }

      small {
        font-size: var(--fs-sm);
        color: var(--text-muted);
      }
    }

    .sheet-action--expense {
      --action: var(--danger-text);
    }

    .sheet-action--income {
      --action: var(--success-text);
    }

    .sheet-action--transfer {
      --action: var(--primary);
    }
  `
})
export class QuickActionSheetComponent {
  protected readonly data = inject<{ canTransfer: boolean }>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<QuickAction | null>>(DialogRef);

  protected pick(action: QuickAction): void {
    this.dialogRef.close(action);
  }
}
