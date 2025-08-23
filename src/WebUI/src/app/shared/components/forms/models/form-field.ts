import { SelectOptionModel } from "./select-option-model"

export interface FormField {
  label: string,
  controlName: string,
  input?: {
    type: 'text' | 'number' | 'checkbox',
    placeholder?: string
  },
  select?: {
    options: SelectOptionModel[],
    optionCaption?: string
  },
  dateTimePicker?: {},
  textArea?: {}
}
