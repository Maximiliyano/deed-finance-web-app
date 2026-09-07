export interface IncomeResponse {
    id: number;
    capitalId: number;
    categoryId: number;
    amount: number;
    paymentDate: Date;
    purpose: string | null;
}
