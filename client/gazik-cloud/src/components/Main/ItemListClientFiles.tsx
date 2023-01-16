import { AiFillFile } from "react-icons/ai";
import { AiFillFolder } from "react-icons/ai";
import { ItemListClientFilesProps } from "../../interfaces/ItemListClientFilesProps";

const ItemListClientFiles = ({
  type,
  name,
  ...props
}: ItemListClientFilesProps): JSX.Element => {
  return (
    <div {...props} className="item-list-client-content">
      {type === "folder" ? (
        <AiFillFolder className="folder-svg" />
      ) : (
        <AiFillFile className="file-svg" />
      )}
      <p className="item-name">{name}</p>
    </div>
  );
};

export default ItemListClientFiles;
