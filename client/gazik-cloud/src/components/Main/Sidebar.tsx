import "../../styles/Sidebar.css";
import SidebarNav from "./SidebarNav";
import Button from "../UI/button/Button";
import { MdUpload } from "react-icons/md";
import { HiPlus } from "react-icons/hi";

const Sidebar = (): JSX.Element => {
  return (
    <section className="sidebar">
      <Button color="yellow" icon={<MdUpload className="sidebar-btn-svg" />}>
        Загрузить
      </Button>
      <Button color="white" icon={<HiPlus className="sidebar-btn-svg" />}>
        Создать
      </Button>
      <SidebarNav />
    </section>
  );
};

export default Sidebar;
