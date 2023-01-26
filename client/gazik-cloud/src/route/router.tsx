import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
} from "react-router-dom";
import ShowPhoto from "../components/main/ShowPhoto";
import ShowStorage from "../components/main/ShowStorage";
import ShowTrash from "../components/main/ShowTrash";
import CheckAuth from "../hoc/CheckAuth";
import MainPage from "../pages/MainPage";
import StartingPage from "../pages/StartingPage";

export const router = createBrowserRouter(
  createRoutesFromElements(
    <Route>
      <Route
        path="/"
        element={
          <CheckAuth>
            <MainPage />
          </CheckAuth>
        }
      >
        <Route path="files" element={<ShowStorage />} />
        <Route path="files/*" element={<ShowStorage />} />
        <Route path="photo" element={<ShowPhoto />} />
        <Route path="trash" element={<ShowTrash />} />
      </Route>
      <Route
        path="auth"
        element={
          <CheckAuth>
            <StartingPage />
          </CheckAuth>
        }
      />
      <Route path="*" element={<div>Not foud page...</div>} />
    </Route>
  )
);
