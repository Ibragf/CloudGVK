import React, { useState } from "react";
import "../../styles/Header.css";
import { AiOutlineCloudUpload, AiFillLock } from "react-icons/ai";
import { VscSignOut } from 'react-icons/vsc';
import { FaSignInAlt } from "react-icons/fa";
import SearchBar from "./SearchBar";
import AuthButton from "../UI/button/AuthButton";
import PopUp from "../popUp/PopUp";
import Blur from "../effects/Blur";
import PopUpAuth from "../popUp/PopUpAuth";
import { useAppDispatch, useAppSelector } from "../../hooks/hooks";
import { notLoggedIn } from "../../store/userProfileSlice";
import { signOutPost } from "../../apis/server";

const Header: React.FC = () => {
  const [authStr, setAuthStr] = useState<string>("");
  const [displayPopUp, setDisplayPopUp] = useState<boolean>(false);
  const isLoggedIn = useAppSelector((state) => state.authReducer.isLoggedIn);
  const dispatch = useAppDispatch();

  function authHandler(e: any) {
    if([...e.target.closest("button").classList][1] === 'signOut') {
        console.log('true');
        (async function(){await signOutPost()})()
      dispatch(notLoggedIn(!isLoggedIn));
    }   
    if ([...e.target.classList].includes("auth-buttons")) return;
    setDisplayPopUp(!displayPopUp);
    return [...e.target.closest("button").classList][1];
  }

  return (
    <div className="header">
      <div className="header-logo">
        <AiOutlineCloudUpload className="header-img" />
        <span className="header-text">Gazik Cloud</span>
      </div>
      <div className="header-input">
        <SearchBar />
      </div>
      <div
        className="auth-buttons"
        onClick={(e) => {
          setAuthStr(authHandler(e)); 
        }}
      >
        {isLoggedIn ? (
          <AuthButton additionClass="signOut"><VscSignOut />Sign out</AuthButton>
        ) : (
          <React.Fragment>
            <AuthButton additionClass="signIn">
              <FaSignInAlt />
              Sign in
            </AuthButton>
            <AuthButton additionClass="signUp">
              <AiFillLock />
              Sign up
            </AuthButton>
          </React.Fragment>
        )}
      </div>
      <PopUp close={setDisplayPopUp} display={displayPopUp} variant={authStr}>
        <PopUpAuth type={authStr} />
      </PopUp>
      <Blur
        onClick={() => setDisplayPopUp(!displayPopUp)}
        display={displayPopUp}
      ></Blur>
    </div>
  );
};

export default Header;
