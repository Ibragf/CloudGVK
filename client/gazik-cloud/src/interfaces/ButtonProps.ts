import { ButtonHTMLAttributes, DetailedHTMLProps, ReactNode } from "react";

export interface ButtonProps
  extends DetailedHTMLProps<
    ButtonHTMLAttributes<HTMLButtonElement>,
    HTMLButtonElement
  > {
  color?: "yellow" | "white";
  children?: ReactNode;
  icon?: JSX.Element;
  optionBtn?: boolean;
  stateOpenList?: boolean;
}
