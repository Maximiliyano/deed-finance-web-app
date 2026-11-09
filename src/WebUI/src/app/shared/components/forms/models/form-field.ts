import { Tag } from "../../../../modules/expense/models/tag";
import { SelectOptionModel } from "./select-option-model"

export interface FormField {
  label: string,
  controlName: string,
  input?: {
    type: 'text' | 'number' | 'checkbox' | 'password',
    placeholder?: string
  },
  selectiveInput?: {
    data: Tag[]
  }
  select?: {
    options: SelectOptionModel[],
    optionCaption?: string
  },
  dateTimePicker?: {
    restrictFuture?: boolean
  },
  textArea?: {
    placeholder?: string;
  }
}
