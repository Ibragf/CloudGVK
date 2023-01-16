import Button from "../UI/button/Button";
import { BsSortUpAlt } from "react-icons/bs";
import { MdKeyboardArrowRight } from "react-icons/md";
import { useEffect, useState } from "react";
import ListSortOptions from "./ListSortOptions";
import ListViewOptions, { findSvg } from "./ListViewOptions";
import { HeaderShowContentProps } from "../../interfaces/HeaderShowContentProps";
import { useNavigate } from "react-router-dom";

const HeaderShowContent = ({
  page,
  titleLink,
}: HeaderShowContentProps): JSX.Element => {
  const sortOptions: string[] = [
    "Sort by name",
    "Sort by size",
    "Sort by date",
    "Sort by type",
  ];
  const viewOptions: string[] = ["Tiles", "Big tiles", "List"];

  const [openSortOptions, setOpenSortOptions] = useState<boolean>(false);
  const [openViewOptions, setOpenViewOptions] = useState<boolean>(false);

  const [activeSortType, setActiveSortType] = useState<string>(sortOptions[0]);
  const [activeViewType, setActiveViewType] = useState<string>(viewOptions[0]);

  useEffect(() => {
    const handler = (e: any) => {
      if (!e.target.closest(".btn-sort")) setOpenSortOptions(false);
      if (!e.target.closest(".btn-view")) setOpenViewOptions(false);
    };
    document.addEventListener("mousedown", handler);
    return () => {
      document.removeEventListener("mousedown", handler);
    };
  });

  const navigation = useNavigate();

  return (
    <div className="header-show-content">
      {titleLink ? (
        <div onClick={() => navigation('/files')} className="name-page-link">
          {page} <MdKeyboardArrowRight className="name-page-svg" />
        </div>
      ) : (
        <div className="name-page">{page}</div>
      )}
      <div className="select-settings">
        <Button
          className="btn-sort"
          onClick={() => setOpenSortOptions(!openSortOptions)}
          color="white"
          optionBtn
          icon={<BsSortUpAlt className="big-svg" />}
          stateOpenList={openSortOptions}
        >
          {activeSortType}
        </Button>

        <Button
          onClick={() => setOpenViewOptions(!openViewOptions)}
          className="btn-view"
          color="white"
          optionBtn
          icon={findSvg(activeViewType)}
          stateOpenList={openViewOptions}
        />

        <ListSortOptions
          options={sortOptions}
          activeType={activeSortType}
          setActiveType={setActiveSortType}
          stateOpen={openSortOptions}
          timeout={180}
        />
        <ListViewOptions
          options={viewOptions}
          activeType={activeViewType}
          setActiveType={setActiveViewType}
          stateOpen={openViewOptions}
          timeout={180}
        />
      </div>
    </div>
  );
};

export default HeaderShowContent;
