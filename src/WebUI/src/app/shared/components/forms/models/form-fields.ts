export interface FormFields {
  label: string,
  controlName: string,
  input?: {
    type: string,
    placeholder: string
  },
  select?: {
    options: string[]
  }
}
