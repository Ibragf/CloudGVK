import ShowFiles from "./ShowFiles";
import Sidebar from "./Sidebar";
import "../styles/Main.css";


const Main = (): JSX.Element => {
  return (
    <main className="main">
      <Sidebar/>
      <ShowFiles/>
    </main>
  );
};

export default Main;
