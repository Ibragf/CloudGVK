import React, { useState } from "react";
import TextInput from "../UI/input/TextInput";
import AuthButton from "../UI/button/AuthButton";
import axios from "axios";

const PopUpSignIn: React.FC = () => {
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');

  const emailHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value)
  }

  const passHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPassword(e.target.value)
  }

  const onSubmitHandler = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    try {
      const response = await axios.post('/api/register', {
        email: email,
        password: password,
      });
      console.log(response.data);
      
    } catch (error) {
      alert(`Something went wrong(${error})`);
    }
  }

  return (
    <form className="popup-form" action="">
      <div>
        <label className="popup-label" htmlFor="email">
          E-mail:
        </label>
        <TextInput type="email" id="email" value={email} onChange={(e) => emailHandler(e)}></TextInput>
      </div>
      <div>
        <label className="popup-label" htmlFor="pass">
          Password:
        </label>
        <TextInput type="password" id="pass" value={password} onChange={(e) => passHandler(e)}></TextInput>
      </div>
      <AuthButton disabled={!email || !password ? true : false} onClick={(e) => onSubmitHandler(e)} backgroundColor="#ffdc60">Sign In</AuthButton>
    </form>
  );
};

export default PopUpSignIn;