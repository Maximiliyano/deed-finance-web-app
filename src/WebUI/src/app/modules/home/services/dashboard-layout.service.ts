import { Injectable } from '@angular/core';

export type PanelId = 'kpi' | 'estimations' | 'debts' | 'goals' | 'capitals' | 'expenses' | 'incomes';

export interface PanelMeta {
  id: PanelId;
  label: string;
  icon: string;
  locked: boolean;
}

const ALL_PANELS: PanelMeta[] = [
  { id: 'kpi', label: 'KPI Cards', icon: 'fa-chart-line', locked: true },
  { id: 'estimations', label: 'Budget Planner', icon: 'fa-compass-drafting', locked: false },
  { id: 'capitals', label: 'Capitals', icon: 'fa-wallet', locked: false },
  { id: 'expenses', label: 'Expenses', icon: 'fa-money-bill-wave', locked: false },
  { id: 'incomes', label: 'Incomes', icon: 'fa-arrow-trend-up', locked: false },
  { id: 'debts', label: 'Debt Dashboard', icon: 'fa-hand-holding-dollar', locked: false },
  { id: 'goals', label: 'Goals', icon: 'fa-star', locked: false },
];

@Injectable({ providedIn: 'root' })
export class DashboardLayoutService {
  private readonly storageKey = 'deed-dashboard-layout';
  private readonly orderKey = 'deed-dashboard-order';
  private visibility: Record<PanelId, boolean>;
  private order: PanelId[];

  private readonly defaults: Record<PanelId, boolean> = {
    kpi: true,
    estimations: true,
    debts: true,
    goals: true,
    capitals: true,
    expenses: true,
    incomes: true,
  };

  constructor() {
    const saved = localStorage.getItem(this.storageKey);
    this.visibility = saved ? { ...this.defaults, ...JSON.parse(saved) } : { ...this.defaults };
    this.visibility['kpi'] = true;

    const savedOrder = localStorage.getItem(this.orderKey);
    const defaultOrder = ALL_PANELS.map(p => p.id);
    if (savedOrder) {
      const parsed: PanelId[] = JSON.parse(savedOrder);
      // Ensure all panels are present (handle new panels added after save)
      this.order = [...parsed.filter(id => defaultOrder.includes(id)), ...defaultOrder.filter(id => !parsed.includes(id))];
    } else {
      this.order = defaultOrder;
    }

    this.orderedPanels = this.order.map(id => ALL_PANELS.find(p => p.id === id)!);
  }

  isVisible(panel: PanelId): boolean {
    if (panel === 'kpi') return true;
    return this.visibility[panel] ?? true;
  }

  toggle(panel: PanelId): void {
    if (panel === 'kpi') return;
    this.visibility[panel] = !this.visibility[panel];
    localStorage.setItem(this.storageKey, JSON.stringify(this.visibility));
  }

  orderedPanels: PanelMeta[] = [];

  get panels(): PanelMeta[] {
    return this.orderedPanels;
  }

  saveOrder(): void {
    this.order = this.orderedPanels.map(p => p.id);
    localStorage.setItem(this.orderKey, JSON.stringify(this.order));
  }
}
