import "./styles/App.css";
import Header from "./components/header/Header";
import Main from "./components/main/Main";
import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
} from "react-router-dom";
import ShowStorage from "./components/main/ShowStorage";
import ShowPhoto from "./components/main/ShowPhoto";
import ShowTrash from "./components/main/ShowTrash";
import CheckAuth from "./hoc/CheckAuth";

const LayoutApp = (): JSX.Element => {
  return (
    <div className="App">
      <Header />
      <Main />
    </div>
  );
};

const Login = (): JSX.Element => {
  return <div className="Login">Login...</div>;
};

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route>
      <Route
        path="/"
        element={
          <CheckAuth>
            <LayoutApp />
          </CheckAuth>
        }
      >
        <Route path="files" element={<ShowStorage />} />
        <Route path="files/*" element={<ShowStorage />} />
        <Route path="photo" element={<ShowPhoto />} />
        <Route path="trash" element={<ShowTrash />} />
      </Route>
      <Route
        path="login"
        element={
          <CheckAuth>
            <Login />
          </CheckAuth>
        }
      />
      <Route path="*" element={<div>Not foud page...</div>} />
    </Route>
  )
);

function App() {
   return <RouterProvider router={router} />;

}

export default App;
