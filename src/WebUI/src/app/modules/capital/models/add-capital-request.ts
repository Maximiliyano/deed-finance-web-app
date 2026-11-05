export interface AddCapitalRequest {
    name: string,
    balance: number,
    currency: string,
    includeInTotal: boolean,
    onlyForSavings: boolean,
}
