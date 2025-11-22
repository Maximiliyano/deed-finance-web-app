import { CategoryType } from "../../../core/types/category-type";
import { PerPeriodType } from "../../../core/types/per-period-type";

export interface CreateCategoryRequest {
    name: string;
    type: CategoryType;
    plannedPeriodAmount: number;
    period: PerPeriodType;
};