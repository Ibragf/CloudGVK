import React from "react";
import "../../styles/Blur.css";
import { BlurProps } from "../../interfaces/BlurProps";

const Blur: React.FC<BlurProps> = ({ display, ...props }) => {
  return (
    <div
      className="blur"
      {...props}
      style={{ display: display ? "block" : "none" }}
    ></div>
  );
};

export default Blur;
