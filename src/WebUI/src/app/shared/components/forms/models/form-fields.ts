import { SelectOptionModel } from "./select-option-model"

export interface FormFields {
  label: string,
  controlName: string,
  input?: {
    type: 'text' | 'number' | 'checkbox',
    placeholder?: string
  },
  autoComplete?: {
    options: string[]
  },
  select?: {
    options: SelectOptionModel[],
    optionCaption?: string
    onChange?: (value: Event) => void,
  },
  dateTimePicker?: {},
  textArea?: {}
}
