import { AiFillFile } from "react-icons/ai";
import { FileProps } from "../../../interfaces/FileProps";
import styles from "./File.module.css";

const File = ({ name }: FileProps): JSX.Element => {
  return (
    <div className={styles.folder}>
      <AiFillFile className="file-svg" />
      <p className={styles.folderName}>{name}</p>
    </div>
  );
};

export default File;
