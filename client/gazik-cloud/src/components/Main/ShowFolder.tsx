import { useParams } from "react-router-dom";
import HeaderShowContent from "./HeaderShowContent";

const ShowFolder = (): JSX.Element => {
  const { folderName } = useParams();
  return (
    <div>
      <HeaderShowContent page="Files" titleLink/>
      {folderName}
    </div>
  );
};

export default ShowFolder;
