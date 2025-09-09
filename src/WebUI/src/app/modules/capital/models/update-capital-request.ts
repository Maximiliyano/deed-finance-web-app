import { CurrencyType } from "../../../core/types/currency-type";

export interface UpdateCapitalRequest {
  id: number;
  name: string | null;
  balance: number | null;
  currency: CurrencyType | null;
  includeInTotal: boolean | null;
  onlyForSavings: boolean | null;
}
