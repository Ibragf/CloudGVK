import { ButtonProps } from "./ButtonProps";
import styles from "./Button.module.css";
import cn from "classnames";
import { MdUpload } from "react-icons/md";
import { HiPlus } from "react-icons/hi";

const Button = ({
  children,
  color = "white",
  icon,
  ...props
}: ButtonProps): JSX.Element => {
  return (
    <button
      className={cn(styles.btn, {
        [styles.white]: color === "white",
        [styles.yellow]: color === "yellow",
      })}
      {...props}
    >
      {icon === "create" ? (
        <HiPlus className={styles.svg} />
      ) : (
        <MdUpload className={styles.svg} />
      )}
      {children}
    </button>
  );
};

export default Button;
