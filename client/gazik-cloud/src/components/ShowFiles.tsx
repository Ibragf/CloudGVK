import BodyFiles from "./BodyFiles";
import HeadShowFiles from "./HeadShowFiles";
import "../styles/ShowFiles.css"

const ShowFiles = (): JSX.Element => {
  return (
    <div className="show-files">
      <HeadShowFiles />
      <BodyFiles />
    </div>
  );
};

export default ShowFiles;
