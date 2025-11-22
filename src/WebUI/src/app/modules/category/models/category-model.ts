import { CategoryType } from "../../../core/types/category-type";
import { PerPeriodType } from "../../../core/types/per-period-type";

export interface CategoryResponse {
  id: number;
  name: string;
  type: CategoryType;
  periodAmount: number;
  periodType: PerPeriodType;
  isDeleted: boolean;
}
