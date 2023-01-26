import React, { useState, useEffect } from "react";
import AuthForm from "../components/startingPage/AuthForm";
import SwitchBtn from "../components/startingPage/switchBtn/SwitchBtn";
import "../styles/StartingPage.css";

const StartingPage = () => {
  const [signUpRotated, setSignUpRotated] = useState<string>("0");
  const [signInRotated, setSignInRotated] = useState<string>("180deg");
  const [isToggled, setIsToggled] = useState<boolean>(false);
  const [canRender, setCanRender] = useState<number>(0);

  useEffect(() => {
    if (canRender) {
      let temp = signInRotated;
      setSignInRotated(signUpRotated);
      setSignUpRotated(temp);
    } else setCanRender(canRender + 1);
  }, [isToggled]);

  const onToggle = () => {
    setIsToggled(!isToggled);
  };

  return (
    <div className="starting-page">
      <div className="starting-page__inner">
      <div className="sign-variant">
        <p>Sign up</p>
        <p>Sign in</p>
      </div>
      <SwitchBtn onToggle={onToggle} />
      <div className="form-container">
        <div className="form__inner">
          <AuthForm type="signUp" rotate={signUpRotated}></AuthForm>
          <AuthForm type="signIn" rotate={signInRotated}></AuthForm>
        </div>
      </div>
      </div>
    </div>
  );
};

export default StartingPage;
