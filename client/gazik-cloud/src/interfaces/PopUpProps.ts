import { ReactNode } from "react";


export interface PopUpProps {
    variant: string,
    display: boolean,
    children: ReactNode,
    close?: any
}

