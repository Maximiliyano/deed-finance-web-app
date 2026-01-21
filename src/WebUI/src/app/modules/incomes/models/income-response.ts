import { CapitalResponse } from "../../capital/models/capital-response";
import { CategoryResponse } from "../../category/models/category-model";

export interface IncomeResponses {
    incomes: IncomeResponse[];
    categories: CategoryResponse[];
    capitals: CapitalResponse[];
}
export interface IncomeResponse {
    id: number;
    capitalId: number;
    categoryId: number;
    amount: number;
    paymentDate: Date;
    purpose: string | null;
}
