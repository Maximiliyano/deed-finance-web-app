export interface UpdateCapitalRequest {
  id: number;
  name: string | null;
  balance: number | null;
  currency: string | null;
  includeInTotal: boolean | null;
}
