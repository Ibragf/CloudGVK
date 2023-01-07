import "../styles/Sidebar.css";
import SidebarNav from "./SidebarNav";
import Button from "./UI/button/Button";

const Sidebar = (): JSX.Element => {
  return (
    <div className="sidebar">
      <Button color="yellow" icon="load">
        Загрузить
      </Button>
      <Button color="white" icon="create">
        Создать
      </Button>
      <SidebarNav />
    </div>
  );
};

export default Sidebar;
