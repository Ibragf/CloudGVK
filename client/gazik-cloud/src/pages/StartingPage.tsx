import React, { useState } from "react";
import SignInForm from "../components/startingPage/SignInForm";
import SignUpForm from "../components/startingPage/SignUpForm";
import "../styles/StartingPage.css";

const StartingPage = () => {
  const [signUpRotated, setSignUpRotated] = useState<boolean>(false);
  const [signInRotated, setSignInRotated] = useState<boolean>(true);
  return (
    <div className="starting-page">
        <button
          onClick={() => {
            setSignInRotated(!signInRotated);
            setSignUpRotated(!signUpRotated);
          }}
        >
          rotate
        </button>
      <div className="form-container">
      
        <div className="form__inner">
        <SignUpForm rotate={signUpRotated} />
        <SignInForm rotate={signInRotated} />
        </div>
      </div>
    </div>
  );
};

export default StartingPage;
