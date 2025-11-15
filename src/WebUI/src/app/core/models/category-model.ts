import {CategoryType} from "../types/category-type";
import { PerPeriodType } from "../types/per-period-type";

export interface CategoryResponse {
  id: number;
  name: string;
  type: CategoryType;
  periodAmount: number;
  periodType: PerPeriodType;
}
