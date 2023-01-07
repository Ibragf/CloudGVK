import { DetailedHTMLProps, SelectHTMLAttributes } from "react";

export interface SelectSortOptionsInterface {
  name: string;
  value: string;
  selected: boolean;
}

export interface SelectSortProps
  extends DetailedHTMLProps<
    SelectHTMLAttributes<HTMLSelectElement>,
    HTMLSelectElement
  > {
  options: SelectSortOptionsInterface[];
}
