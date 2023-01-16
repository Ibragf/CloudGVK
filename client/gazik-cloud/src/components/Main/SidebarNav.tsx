import { AiFillFile } from "react-icons/ai";
import { HiPhotograph } from "react-icons/hi";
import { BiTrash } from "react-icons/bi";
import { NavLink } from "react-router-dom";



const SidebarNav = (): JSX.Element => {
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
          <NavLink
            to={item.name.toLowerCase()}
            className={({ isActive }) =>
              isActive ? "link-page active-page" : "link-page"
            }
            key={`${item.name}-${i}`}
          >
            <li
              className="item-list-sidebar"
            >
              {item.icon}
              {item.name}
            </li>
          </NavLink>
        ))}
      </ul>
    </div>
  );
};

export default SidebarNav;

