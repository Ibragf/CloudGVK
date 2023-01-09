import { ButtonHTMLAttributes, DetailedHTMLProps, ReactNode } from 'react';

export interface AuthBtnProps extends DetailedHTMLProps<
ButtonHTMLAttributes<HTMLButtonElement>,
HTMLButtonElement> {
    backgroundColor?: string, 
    borderRadius?: string, 
    width?: string,
    height?: string,
    children: ReactNode,
    additionClass?: string,
}