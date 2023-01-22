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
  Routes,
} from "react-router-dom";
import ShowStorage from "./ShowStorage";
import ShowTrash from "./ShowTrash";
import ShowPhoto from "./ShowPhoto";
import ShowAbout from "./ShowAbout";
import ShowFolder from "./ShowFolder";

const LayoutMain = (): JSX.Element => {
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

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route path="/" element={<LayoutMain />}>
      <Route index element={<ShowAbout />} />
      <Route path="files" element={<ShowStorage />} />
      <Route path="files/*" element={<ShowFolder />} />
      <Route path="photo" element={<ShowPhoto />} />
      <Route path="trash" element={<ShowTrash />} />
    </Route>
  )
);

const Main = (): JSX.Element => {
  return <RouterProvider router={router} />;
};

export default Main;



