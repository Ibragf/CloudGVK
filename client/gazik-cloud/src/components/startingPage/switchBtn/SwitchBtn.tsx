import React, { useState } from "react";
import "./SwitchBtn.css";
import { SwitchBtnProps } from "./SwitchBtnProps";

const SwitchBtn: React.FC<SwitchBtnProps> = ({ onToggle }) => {
  const [isToggled, setIsToggled] = useState<boolean>(false);

  return (
    <div className="switch-btn">
      <div className="switch-container">
        <div
          className={`toggle-btn ${isToggled ? "move-right" : "move-left"}`}
          onClick={() => {
            onToggle();
            setIsToggled(!isToggled);
          }}
        ></div>
      </div>
    </div>
  );
};

export default SwitchBtn;
