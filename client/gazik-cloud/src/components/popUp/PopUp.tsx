import React from "react";
import "../../styles/Popup.css";
import { PopUpProps } from "../../interfaces/PopUpProps";
import { AiOutlineClose } from 'react-icons/ai';



const PopUp: React.FC<PopUpProps> = ({display, children, variant, close}) => {
  return (
    <div className={`popup ${variant}-popup`} style={{display: display ? 'block' : 'none'}}>
      <div className="popup-container">
          <AiOutlineClose onClick={() => close(!display)} className='close-icon'></AiOutlineClose>
          {children}
      </div>
    </div>
  );
};

export default PopUp;
