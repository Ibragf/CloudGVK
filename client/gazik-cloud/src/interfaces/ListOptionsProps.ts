export interface ListOptionsProps {
  stateOpen: boolean;
  timeout: number;
  setActiveType: (type: string) => void;
  activeType: string;
  options: string[];
}


