import React, { useEffect, useState } from "react";
import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import axios from '../../apis/server';

// States
const PopUpSignUp: React.FC = () => {
  const [name, setName] = useState<string>("");
  const [email, setEmail] = useState<string>("");
  const [dirtyEmail, setDirtyEmail] = useState<boolean>(false);
  const [emailError, setEmailError] = useState<string>(
    "The field should not be empty"
  );
  const [password, setPassword] = useState<string>("");
  const [dirtyPass, setDirtyPass] = useState<boolean>(false);
  const [passError, setPassError] = useState<string>(
    "The field should not be empty"
  );
  const [formValid, setFormValid] = useState<boolean>(false);

  useEffect(() => {
    if(emailError || passError) setFormValid(false);
    else setFormValid(true);
  }, [emailError, passError])


  //Handlers

  const emailHandler = (e: any) => {
    setEmail(e.target.value);
    const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

    if(!re.test(String(e.target.value).toLocaleLowerCase())) {
      setEmailError('Wrong email!');
    } else setEmailError('')
  }

  const passHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPassword(e.target.value);
    const re =  /^[a-zA-Z0-9!@#$%^&*]{6,16}$/;

    if(!re.test(String(e.target.value))) {
      setPassError('Wrong password!')
    } else setPassError('');
  }

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

  const onSubmitHandler = async(e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();

    try {
      const response = await axios.post('/api/signup', {
        userName: name,
        email: email,
        password: password,
      });
      console.log(response);

    } catch (error) {
      alert(`Something went wrong(${error})`);
    }
  }

  return (
    <form className="popup-form" action="">
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
      <div>
        <label className="popup-label" htmlFor="email">
          E-mail:
        </label>
        {dirtyEmail && emailError ? (
          <div style={{ color: "red" }}>{emailError}</div>
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
          <div style={{ color: "red" }}>{passError}</div>
        ) : null}
        <TextInput
          onBlur={(e) => blurHandler(e)}
          type="password"
          id="pass"
          value={password}
          onChange={(e) => passHandler(e)}
        ></TextInput>
      </div>
      <AuthButton onClick={(e) => onSubmitHandler(e)} disabled={!formValid} backgroundColor="#ffdc60">Submit</AuthButton>
    </form>
  );
};

export default PopUpSignUp;
