import Sidebar from "./Sidebar";
import HeaderShowFiles from "./HeaderShowFiles";
import MainShowFiles from "./MainShowFiles";
import "../../styles/Main.css";
import "../../styles/ShowFiles.css";

const Main = (): JSX.Element => {
  return (
    <main className="main">
      <Sidebar />
      <section className="show-files">
        <HeaderShowFiles />
        <MainShowFiles />
      </section>
    </main>
  );
};

export default Main;
