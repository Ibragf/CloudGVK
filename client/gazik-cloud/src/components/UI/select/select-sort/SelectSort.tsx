import { SelectSortProps } from "./SelectSortProps";

const SelectSort = ({ options }: SelectSortProps): JSX.Element => {
  return (
    <select>
      {options.map((option, i) => (
        <option value={option.value} key={`${option.name}-${i}`}>
          {option.name}
        </option>
      ))}
    </select>
  );
};

export default SelectSort;
