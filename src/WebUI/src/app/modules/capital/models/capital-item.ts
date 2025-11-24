import { CapitalResponse } from "./capital-response";

export type CapitalItem = {
  key: keyof CapitalResponse;
  title: string;
  icon: string;
  style: string;
  url?: string;
};
