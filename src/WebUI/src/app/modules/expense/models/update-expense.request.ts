export interface UpdateExpenseRequest {
    id: number;
    capitalId?: number;
    categoryId?: number;
    amount?: number;
    purpose?: string;
    tagNames?: string[];
    date?: string | null; 
}
