export interface Income {
    id: number;
    capitalId: number;
    amount: number;
    purpose: string;
    type: string;
    createdAt: Date;
}

export interface AddIncomeRequest {
    capitalId: number;
    amount: number;
    type: string;
    purpose: string;
}
