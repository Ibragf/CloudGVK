import { DetailedHTMLProps, InputHTMLAttributes } from "react";

export interface TextInputProps extends DetailedHTMLProps<
InputHTMLAttributes<HTMLInputElement>,
HTMLInputElement
> {
    type: string,
    id?: string,
    width?: string,
    borderRadius?: string,
    value?: string,
}