import React, { useState, useEffect } from "react";

import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import { signUpPost, signInPost } from "../../apis/server";
import { useAppSelector, useAppDispatch } from "../../hooks/hooks";
import { notLoggedIn, setEmail, setName, setPassword } from "../../store/userProfileSlice";
import '../../styles/SignUpForm.css';
import { SignUpProps } from "./interfaces/SignInProps";

const SignUpForm: React.FC<SignUpProps> = ({rotate}) => {
  const isLoggedIn = useAppSelector((state) => state.authReducer.isLoggedIn);
  const dispatch = useAppDispatch();
  const name = useAppSelector((state) => state.authReducer.name);
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
      await signUpPost(name, email, password);
      await signInPost(email, password);
      dispatch(notLoggedIn(!isLoggedIn));
    } catch (error: any) {
      console.log(error);
    }
  };

  return (
    <form className={`signUp-form ${rotate ? 'signUp-form_rotated' : ''}`} action="">
      <div className="signUp-title">
        <span>Sign up</span>
      </div>
      <div>
        <TextInput
          placeholder="Your Full Name"
          type="text"
          id="name"
          value={name}
          onChange={(e) => dispatch(setName(e.target.value))}
        ></TextInput>
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
    </form>
  );
};

export default SignUpForm;
