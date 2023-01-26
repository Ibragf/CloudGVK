import ButtonLoad from "./ButtonLoad";
import ButtonCreate from "./ButtonCreate";
import SidebarNav from "./SidebarNav";
import "../../styles/Main.css";
import "../../styles/ShowContent.css";
import "../../styles/Sidebar.css";
import { Outlet } from "react-router-dom";

const Main = (): JSX.Element => {
  return (
    <main className="main">
      <section className="sidebar">
        <ButtonLoad />
        <ButtonCreate />
        <SidebarNav />
      </section>
      <section className="show-content">
        <Outlet />
      </section>
    </main>
  );
};
export default Main;
