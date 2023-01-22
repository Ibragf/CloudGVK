import { DetailedHTMLProps, HtmlHTMLAttributes } from "react";
import { UserStorageItem } from "../store/interfaces/IUserStorage";

export interface StorageItemProps
  extends DetailedHTMLProps<
    HtmlHTMLAttributes<HTMLDivElement>,
    HTMLDivElement
  > {
    storageItem: UserStorageItem;
}
