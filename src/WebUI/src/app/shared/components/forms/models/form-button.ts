export interface FormButton {
  type: 'submit' | 'button',
  text: string;
  styles?: string;
  disabled?: boolean;
  onClick?: () => void;
};
