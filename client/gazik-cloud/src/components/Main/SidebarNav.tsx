import { AiFillFile } from "react-icons/ai";
import { HiPhotograph } from "react-icons/hi";
import { BiTrash } from "react-icons/bi";
import cn from "classnames";
import { Link } from "react-router-dom";
import { SidebarNavProps } from "../../interfaces/SidebarNavProps";

const SidebarNav = ({
  activePage,
  setActivePage,
}: SidebarNavProps): JSX.Element => {
  const listPages: {
    name: string;
    icon?: JSX.Element;
  }[] = [
    { name: "Files", icon: <AiFillFile className="sidebar-svg" /> },
    { name: "Photo", icon: <HiPhotograph className="sidebar-svg" /> },
    { name: "Trash", icon: <BiTrash className="sidebar-svg" /> },
  ];

  return (
    <div className="sidebar-nav">
      <ul className="list-sidebar">
        {listPages.map((item, i) => (
          <Link
            to={item.name === "Files" ? "" : item.name.toLowerCase()}
            className="link-page"
          >
            <li
              className={cn("item-list-sidebar", {
                "active-page": item.name === activePage,
              })}
              onClick={() => setActivePage(item.name)}
              key={`${item.name}-${i}`}
            >
              {item.icon}
              {item.name}
            </li>
          </Link>
        ))}
      </ul>
    </div>
  );
};

export default SidebarNav;
