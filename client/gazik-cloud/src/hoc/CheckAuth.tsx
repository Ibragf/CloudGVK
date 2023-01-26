import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAppSelector } from "../hooks/hooks";
import { CheckAuthProps } from "../interfaces/CheckAuthProps";

const CheckAuth = ({ children }: CheckAuthProps): JSX.Element => {
  let { pathname } = useLocation();
  if (pathname === "/" || pathname === "/auth") pathname = "/files";

  const isLoggedIn = useAppSelector((state) => state.authReducer.isLoggedIn);
  const navigate = useNavigate();
  useEffect(() => {
    isLoggedIn ? navigate(pathname) : navigate("/auth");
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoggedIn]);

  return <>{children}</>;
};

export default CheckAuth;
