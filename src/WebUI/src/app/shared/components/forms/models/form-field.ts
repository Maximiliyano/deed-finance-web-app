import { SelectOptionModel } from "./select-option-model"

export interface FormField {
  label: string,
  controlName: string,
  input?: {
    type: 'text' | 'number' | 'checkbox' | 'password',
    placeholder?: string
  },
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
