export interface Exchange {
    targetCurrency: string,
    nationalCurrency: string,
    buy: number,
    sale: number,
    updatedAt: Date;
}