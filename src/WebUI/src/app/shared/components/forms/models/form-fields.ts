import { SelectOptionModel } from "./select-option-model"

export interface FormFields {
  label: string,
  controlName: string,
  input?: {
    type: 'text' | 'number',
    placeholder?: string
  },
  select?: {
    options: SelectOptionModel[],
    optionCaption?: string
  },
  dateTimePicker?: {
  },
  textArea?: {
  }
}
