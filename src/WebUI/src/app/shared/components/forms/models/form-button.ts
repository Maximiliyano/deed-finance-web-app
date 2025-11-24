export interface FormButton {
  type: 'submit' | 'button',
  text: string;
  styles?: string;
  disabled?: boolean;
  visible?: boolean;
  onClick?: () => void;
}
