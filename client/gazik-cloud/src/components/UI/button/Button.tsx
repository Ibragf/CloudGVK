import { ButtonProps } from "../../../interfaces/ButtonProps";
import styles from "./Button.module.css";
import cn from "classnames";
import { IoIosArrowDown } from "react-icons/io";


const Button = ({
  children,
  color = "white",
  icon,
  optionBtn = false,
  stateOpenList,
  className,
  ...props
}: ButtonProps): JSX.Element => {
  return (
    <button
      {...props}
      className={cn(styles.btn, className, "shadow", {
        [styles.white]: color === "white",
        [styles.yellow]: color === "yellow",
        [styles.optionBtn]: optionBtn,
      })}
    >
      {icon}
      {children}
      {optionBtn && (
        <IoIosArrowDown
          className={cn(styles.arrowDown, {
            [styles.arrowReverse]: stateOpenList,
          })}
        />
      )}
    </button>
  );
};

export default Button;
