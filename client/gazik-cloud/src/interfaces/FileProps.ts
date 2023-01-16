import { DetailedHTMLProps, HtmlHTMLAttributes } from "react";

export interface FileProps
  extends DetailedHTMLProps<
    HtmlHTMLAttributes<HTMLDivElement>,
    HTMLDivElement
  > {
  name: string;
}