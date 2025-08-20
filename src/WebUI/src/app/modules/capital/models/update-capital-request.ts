export interface UpdateCapitalRequest {
  name: string | null;
  balance: number | null;
  currency: string | null;
  includeInTotal: boolean | null;
}
