import React from "react";
import "./styles/App.css";
import Header from "./components/header/Header";
import StartingPage from "./pages/StartingPage";
import Main from "./components/main/Main";

function App() {
  return (
    <div className="App">
      {/* <Header />
      <Main /> */}

      <StartingPage />
    </div>
  );
}

export default App;
