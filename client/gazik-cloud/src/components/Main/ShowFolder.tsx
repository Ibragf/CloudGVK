import { useLocation, useNavigate, useParams } from "react-router-dom";
import HeaderShowContent from "./HeaderShowContent";

const ShowFolder = (): JSX.Element => {
  // const { folderName } = useParams();
  const { pathname } = useLocation();
  console.log(useLocation());
  // const { *: folderName } = useParams();
  const nav = useNavigate();

  return (
    <div>
      <HeaderShowContent page="Files" titleLink />
      {pathname}
      <div onClick={() => nav("link")}>linlk</div>
    </div>
  );
};

export default ShowFolder;
