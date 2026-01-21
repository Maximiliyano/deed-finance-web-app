export interface CreateIncomeRequest {
    capitalId: number;
    categoryId: number;
    amount: number;
    paymentDate: Date;
    purpose: string | null;
}