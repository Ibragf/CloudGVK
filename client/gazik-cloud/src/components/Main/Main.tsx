import HeaderShowFiles from "./HeaderShowFiles";
import MainShowFiles from "./MainShowFiles";
import ButtonLoad from "./ButtonLoad";
import ButtonCreate from "./ButtonCreate";
import SidebarNav from "./SidebarNav";
import "../../styles/Main.css";
import "../../styles/ShowFiles.css";
import "../../styles/Sidebar.css";

const Main = (): JSX.Element => {
  return (
    <main className="main">
      <section className="sidebar">
        <ButtonLoad />
        <ButtonCreate />
        <SidebarNav />
      </section>
      <section className="show-files">
        <HeaderShowFiles />
        <MainShowFiles />
      </section>
    </main>
  );
};

export default Main;
