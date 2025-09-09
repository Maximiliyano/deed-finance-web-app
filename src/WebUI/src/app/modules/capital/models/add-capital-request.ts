import {CurrencyType} from "../../../core/types/currency-type";

export interface AddCapitalRequest {
    name: string,
    balance: number,
    currency: CurrencyType,
    includeInTotal: boolean,
    onlyForSavings: boolean,
}
