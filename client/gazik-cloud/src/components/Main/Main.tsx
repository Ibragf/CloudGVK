import HeaderShowContent from "./HeaderShowContent";
import ButtonLoad from "./ButtonLoad";
import ButtonCreate from "./ButtonCreate";
import SidebarNav from "./SidebarNav";
import "../../styles/Main.css";
import "../../styles/ShowContent.css";
import "../../styles/Sidebar.css";
import {
  createBrowserRouter,
  RouterProvider,
  createRoutesFromElements,
  Outlet,
  Route,
} from "react-router-dom";
import ShowFiles from "./ShowFiles";
import ShowTrash from "./ShowTrash";
import ShowPhoto from "./ShowPhoto";
import { useState } from "react";

const LayoutMain = (): JSX.Element => {
  
  const [activePage, setActivePage] = useState<string>("Files");
  return (
    <main className="main">
      <section className="sidebar">
        <ButtonLoad />
        <ButtonCreate />
        <SidebarNav activePage={activePage} setActivePage={setActivePage}/>
      </section>
      <section className="show-content">
        <HeaderShowContent page={activePage} />
        <Outlet />
      </section>
    </main>
  );
};

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route path="/" element={<LayoutMain />}>
      <Route index element={<ShowFiles />} />
      <Route path="/photo" element={<ShowPhoto />} />
      <Route path="/trash" element={<ShowTrash />} />
    </Route>
  )
);

const Main = (): JSX.Element => {
  return <RouterProvider router={router} />;
};

export default Main;
