export interface UserStorageItem {
  id: number;
  type: "file" | "folder";
  name: string;
  body?: UserStorageItem[];
}
