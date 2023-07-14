import { AuthBtnProps } from "../../../interfaces/AuthBtnProps";
import classes from './AuthButton.module.css';

const AuthButton: React.FC<AuthBtnProps> = ({
  borderRadius,
  backgroundColor,
  children,
  width,
  height,
  additionClass,
  ...props
}) => {
  return (
    <button
      {...props}
      className={`${classes.authBtn} ${additionClass}`}
      style={{ width: width, height: height, borderRadius: borderRadius, backgroundColor: backgroundColor }}
    >
      {children}
    </button>
  );
};

export default AuthButton;
