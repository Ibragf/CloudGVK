import React, { useState, useEffect } from "react";
import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import {  signInPost } from "../../apis/server";
import { useAppSelector, useAppDispatch } from "../../hooks/hooks";
import {
  notLoggedIn,
  setEmail,
  setPassword,
} from "../../store/userProfileSlice";
import '../../styles/SignInForm.css';
import { SignInProps } from "./interfaces/SignInProps";

const SignInForm: React.FC<SignInProps> = ({rotate}) => {
  const isLoggedIn = useAppSelector((state) => state.authReducer.isLoggedIn);
  const dispatch = useAppDispatch();
  const email = useAppSelector((state) => state.authReducer.email);
  const password = useAppSelector((state) => state.authReducer.password);
  const [dirtyEmail, setDirtyEmail] = useState<boolean>(false);
  const [emailError, setEmailError] = useState<string>(
    `The field should not be empty`
  );
  const [dirtyPass, setDirtyPass] = useState<boolean>(false);
  const [passError, setPassError] = useState<string>(
    "The field should not be empty"
  );
  const [formValid, setFormValid] = useState<boolean>(false);

  useEffect(() => {
    if (emailError || passError) setFormValid(false);
    else setFormValid(true);
  }, [emailError, passError]);

  //Handlers

  const emailHandler = (e: any) => {
    dispatch(setEmail(e.target.value));
    const re =
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

    if (!re.test(String(e.target.value).toLocaleLowerCase())) {
      setEmailError(
        `E-mail has to contain "@" sign and generic domain(.com, .ru etc)`
      );
    } else setEmailError("");
  };

  const passHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(setPassword(e.target.value));
    const re = /^[a-zA-Z0-9!@#$%^&*]{10,16}$/;

    if (!re.test(String(e.target.value))) {
      setPassError("Password has to contain at least 6 characters");
    } else setPassError("");
  };

  const blurHandler = (e: any) => {
    switch (e.target.id) {
      case "email":
        setDirtyEmail(true);
        break;

      case "pass":
        setDirtyPass(true);
        break;
    }
  };

  const onSubmitHandler = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();

    try {
      await signInPost(email, password);
      dispatch(notLoggedIn(!isLoggedIn));
    } catch (error: any) {
      console.log(error);
    }
  };

  return (
    <form className={`signIn-form ${rotate ? '' : 'signIn-form_rotated'}`} action="">
      <div className="signIn-title">
        <span>Sign in</span>
      </div>
      <div>
        {dirtyEmail && emailError ? (
          <div style={{ color: "red", fontSize: ".8rem", marginBottom: "5px" }}>
            {emailError}
          </div>
        ) : null}
        <TextInput
          placeholder="Your email"
          onBlur={(e) => blurHandler(e)}
          type="email"
          id="email"
          value={email}
          onChange={(e) => emailHandler(e)}
        ></TextInput>
      </div>
      <div>
        {dirtyPass && passError ? (
          <div style={{ color: "red", fontSize: ".8rem", marginBottom: "5px" }}>
            {passError}
          </div>
        ) : null}
        <TextInput
          placeholder="Your password"
          onBlur={(e) => blurHandler(e)}
          type="password"
          id="pass"
          value={password}
          onChange={(e) => passHandler(e)}
        ></TextInput>
      </div>
      <AuthButton
        onClick={(e) => onSubmitHandler(e)}
        disabled={!formValid}
        backgroundColor="#ffebae"
      >
        Submit
      </AuthButton>
      <div><a href="#">Forgot your password?</a></div>
    </form>
  );
};

export default SignInForm;
