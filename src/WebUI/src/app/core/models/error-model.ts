import { HttpErrorResponse } from "@angular/common/http";

export interface Error {
    message: string;
}

export interface AppError {
  status: number;
  message: string;
  errors?: string[];
  redirectTo?: string;
  originalError: HttpErrorResponse;
}
