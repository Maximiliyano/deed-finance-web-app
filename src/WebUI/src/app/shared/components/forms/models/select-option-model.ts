import { CurrencyType } from "../../../../core/types/currency-type";

export interface SelectOptionModel {
  key: string | number;
  value: string | number | CurrencyType;
}
