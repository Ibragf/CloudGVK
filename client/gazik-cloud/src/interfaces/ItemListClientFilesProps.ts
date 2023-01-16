import { DetailedHTMLProps, HtmlHTMLAttributes } from "react";

export interface ItemListClientFilesProps
  extends DetailedHTMLProps<
    HtmlHTMLAttributes<HTMLDivElement>,
    HTMLDivElement
  > {
  type: "folder" | "file";
  name: string;
  body?: ItemListClientFilesProps[];
}
