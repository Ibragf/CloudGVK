import React, { useEffect, useState } from "react";
import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import { signInPost, signUpPost } from "../../apis/server";
import { useAppDispatch, useAppSelector } from "../../hooks/hooks";
import {
  notLoggedIn,
  setEmail,
  setName,
  setPassword,
} from "../../store/userProfileSlice";
import { AuthFormProps } from "./interfaces/AuthFormProps";
import "../../styles/AuthForm.css";
import { AiOutlineUser } from "react-icons/ai";
import { MdAlternateEmail } from "react-icons/md";
import { RiLockPasswordLine } from "react-icons/ri";

const AuthForm: React.FC<AuthFormProps> = ({ type, rotate }) => {
  const isLoggedIn = useAppSelector((state) => state.authReducer.isLoggedIn);
  const dispatch = useAppDispatch();
  const userData = useAppSelector(state => state.authReducer.userData);
  const {name, email, password} = userData;

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
      case `email-${type}`:
        setDirtyEmail(true);
        break;

      case `pass-${type}`:
        setDirtyPass(true);
        break;
    }
  };

  const onSubmitHandler = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();

    try {
      if (type === "signUp") {
        await signUpPost(name, email, password);
        await signInPost(email, password);
        dispatch(notLoggedIn(!isLoggedIn));
      } else {
        await signInPost(email, password);
        dispatch(notLoggedIn(!isLoggedIn));
      }
    } catch (error: any) {
      console.log(error);
    }
  };

  return (
    <form
      className={`auth-form`}
      action=""
      style={{ transform: `rotateY(${rotate})` }}
    >
      <div className="auth-title">
        <span>{type === "signIn" ? "Sign in" : "Sign up"}</span>
      </div>
      {type === "signUp" ? (
        <div className="form-item">
          <AiOutlineUser></AiOutlineUser>
          <TextInput
            placeholder="Your Name"
            type="text"
            id="name"
            value={name}
            onChange={(e) => dispatch(setName(e.target.value))}
          ></TextInput>
        </div>
      ) : null}

      {dirtyEmail && emailError ? (
        <div style={{ color: "red", fontSize: ".8rem" }}>{emailError}</div>
      ) : null}
      <div className="form-item">
        <MdAlternateEmail />
        <TextInput
          placeholder="Your Email"
          onBlur={(e) => blurHandler(e)}
          type="email"
          id={`email-${type}`}
          value={email}
          onChange={(e) => emailHandler(e)}
        ></TextInput>
      </div>
      {dirtyPass && passError ? (
        <div style={{ color: "red", fontSize: ".8rem" }}>{passError}</div>
      ) : null}
      <div className="form-item">
        <RiLockPasswordLine />
        <TextInput
          placeholder="Your Password"
          onBlur={(e) => blurHandler(e)}
          type="password"
          id={`pass-${type}`}
          value={password}
          onChange={(e) => passHandler(e)}
        ></TextInput>
      </div>
      <AuthButton
        onClick={(e) => onSubmitHandler(e)}
        disabled={!formValid}
        backgroundColor="#ffe8a9"
      >
        Submit
      </AuthButton>
      {type === "signIn" ? (
        <div>
          <a className="link auth-link" href="#">
            Forgot your password?
          </a>
        </div>
      ) : null}
    </form>
  );
};

export default AuthForm;
