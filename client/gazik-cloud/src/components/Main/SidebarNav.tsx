import { AiFillFile } from "react-icons/ai";
import { HiPhotograph } from "react-icons/hi";
import { BiTrash } from "react-icons/bi";

const SidebarNav = (): JSX.Element => {
  const listItems: {
    name: string;
    icon?: JSX.Element;
  }[] = [
    { name: "Файлы", icon: <AiFillFile className="sidebar-svg" /> },
    { name: "Фото", icon: <HiPhotograph className="sidebar-svg" /> },
    { name: "Корзина", icon: <BiTrash className="sidebar-svg" /> },
  ];

  return (
    <div className="sidebar-nav">
      <ul className="list-sidebar">
        {listItems.map((item, i) => (
          <li className="item-list-sidebar" key={`${item.name}-${i}`}>
            {item.icon}
            <span>{item.name}</span>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default SidebarNav;
