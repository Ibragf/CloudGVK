import { Transition } from "react-transition-group";
import { AiOutlineCheck } from "react-icons/ai";
import { ListOptionsProps } from "../../interfaces/ListOptionsProps";
import cn from "classnames";

const ListSortOptions = ({
  stateOpen,
  timeout,
  setActiveType,
  options,
  activeType,
}: ListOptionsProps) => {
  const choiceType = (e: any) => {
    // if (!(e.target instanceof HTMLElement)) return;
    const itemList = e.target.closest(".drop-down-list-item");
    if (itemList && itemList.textContent) setActiveType(itemList.textContent);
  };

  return (
    <Transition in={stateOpen} timeout={timeout} unmountOnExit>
      {(state) => (
        <ul
          onClick={choiceType}
          className={`sort-list drop-down-list shadow ${state}`}
        >
          {options.map((option, i) => (
            <li
              className="sort-list-item drop-down-list-item"
              key={`${option}-${i}`}
            >
              <AiOutlineCheck
                className={cn({ invisible: activeType !== option })}
              />
              {option}
            </li>
          ))}
        </ul>
      )}
    </Transition>
  );
};

export default ListSortOptions;
