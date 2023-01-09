import { DetailedHTMLProps, HtmlHTMLAttributes } from "react";

export interface BlurProps
  extends DetailedHTMLProps<
    HtmlHTMLAttributes<HTMLDivElement>,
    HTMLDivElement
  > {
  display: boolean;
}
