import { AiFillFolder } from "react-icons/ai";
import { FolderProps } from "../../../interfaces/FolderProps";
import styles from "./Folder.module.css";

const Folder = ({ name }: FolderProps): JSX.Element => {
  return (
    <div className={styles.folder}>
    	<AiFillFolder className="folder-svg" />
      <p className={styles.folderName}>{name}</p>
    </div>
  );
};

export default Folder;
