import React, { useEffect, useState } from "react";
import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import axios from "../../apis/server";
import { PopUpAuthProps } from "../../interfaces/PopUpAuthProps";
import FingerprintJS from '@fingerprintjs/fingerprintjs'

const PopUpAuth: React.FC<PopUpAuthProps> = ({ type }) => {
  const fpPromise = FingerprintJS.load();

  const [name, setName] = useState<string>("");
  const [email, setEmail] = useState<string>("");
  const [dirtyEmail, setDirtyEmail] = useState<boolean>(false);
  const [emailError, setEmailError] = useState<string>(
    `The field should not be empty`
  );
  const [password, setPassword] = useState<string>("");
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
    setEmail(e.target.value);
    const re =
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

    if (!re.test(String(e.target.value).toLocaleLowerCase())) {
      setEmailError(
        `E-mail has to contain "@" sign and generic domain(.com, .ru etc)`
      );
    } else setEmailError("");
  };

  const passHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPassword(e.target.value);
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
      if (type === "signUp") {
        const response = await axios.post("/api/signup", {
          userName: name,
          email: email,
          password: password,
        });

        console.log(response);
      } else {

        const fp = await fpPromise;
        const result = await fp.get();
        const response = await axios.post("/api/login", {
          fingerPrint: result.visitorId,
          email: email,
          password: password,
        });
        localStorage.setItem('token', response.data);

        const response1 = await axios.get('/WeatherForecast', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        })
        console.log(response);
        console.log(response1);
      }
    } catch (error: any) {
        console.log(error);    
    }
  };

  return (
    <form className="popup-form" action="">
      <div className="popup-title">
        <span>{type === "signIn" ? "Sign in" : "Registration"}</span>
      </div>
      {type === "signUp" ? (
        <div>
          <label className="popup-label" htmlFor="name">
            Name:
          </label>
          <TextInput
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          ></TextInput>
        </div>
      ) : null}
      <div>
        <label className="popup-label" htmlFor="email">
          E-mail:
        </label>
        {dirtyEmail && emailError ? (
          <div style={{ color: "red", fontSize: ".8rem", marginBottom: "5px" }}>
            {emailError}
          </div>
        ) : null}
        <TextInput
          onBlur={(e) => blurHandler(e)}
          type="email"
          id="email"
          value={email}
          onChange={(e) => emailHandler(e)}
        ></TextInput>
      </div>
      <div>
        <label className="popup-label" htmlFor="pass">
          Password:
        </label>
        {dirtyPass && passError ? (
          <div style={{ color: "red", fontSize: ".8rem", marginBottom: "5px" }}>
            {passError}
          </div>
        ) : null}
        <TextInput
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
        backgroundColor="#ffdc60"
      >
        Submit
      </AuthButton>
    </form>
  );
};

export default PopUpAuth;
