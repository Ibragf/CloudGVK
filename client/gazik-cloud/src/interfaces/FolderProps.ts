import { DetailedHTMLProps, HtmlHTMLAttributes } from "react";

export interface FolderProps
  extends DetailedHTMLProps<
    HtmlHTMLAttributes<HTMLDivElement>,
    HTMLDivElement
  > {
  name: string;
}