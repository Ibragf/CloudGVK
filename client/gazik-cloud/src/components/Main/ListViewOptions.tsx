import { Transition } from "react-transition-group";
import { ListOptionsProps } from "../../interfaces/ListOptionsProps";
import { AiOutlineCheck } from "react-icons/ai";
import { CgViewDay } from "react-icons/cg";
import { CgViewList } from "react-icons/cg";
import { CgViewGrid } from "react-icons/cg";
import cn from "classnames";

export const findSvg = (nameViewType: string): JSX.Element => {
  switch (nameViewType) {
    case "Tiles":
      return <CgViewGrid className="big-svg tiles" />;
    case "Big tiles":
      return <CgViewDay className="big-svg big-tiles" />;
    case "List":
      return <CgViewList className="big-svg list-tiles" />;
    default:
      return <></>;
  }
};

const ListViewOptions = ({
  stateOpen,
  timeout,
  setActiveType,
  options,
  activeType,
}: ListOptionsProps): JSX.Element => {
  const choiceType = (e: any) => {
    // if (!(e.target instanceof HTMLImageElement)) return;
    const itemList = e.target.closest(".drop-down-list-item");
    if (itemList && itemList.textContent) setActiveType(itemList.textContent);
  };

  return (
    <Transition in={stateOpen} timeout={timeout} unmountOnExit>
      {(state) => (
        <ul
          onClick={choiceType}
          className={`view-list drop-down-list shadow ${state}`}
        >
          {options.map((option, i) => (
            <li
              className="view-list-item drop-down-list-item"
              key={`${option}-${i}`}
            >
              {findSvg(option)}
              {option}
              <AiOutlineCheck
                className={cn({ invisible: activeType !== option })}
              />
            </li>
          ))}
        </ul>
      )}
    </Transition>
  );
};

export default ListViewOptions;
