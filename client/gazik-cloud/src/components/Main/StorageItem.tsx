import { AiFillFile } from "react-icons/ai";
import { AiFillFolder } from "react-icons/ai";
import { useNavigate } from "react-router-dom";
import { StorageItemProps } from "../../interfaces/StorageItemProps";

const StorageItem = ({
  storageItem,
  ...props
}: StorageItemProps): JSX.Element => {
  const navigate = useNavigate();
  
  return (
    <div
      onClick={() => {
        if (storageItem.type === "folder") navigate(storageItem.name);
      }}
      {...props}
      className="storage-item"
    >
      {storageItem.type === "folder" ? (
        <AiFillFolder className="folder-svg" />
      ) : (
        <AiFillFile className="file-svg" />
      )}
      <p className="item-name">{storageItem.name}</p>
    </div>
  );
};

export default StorageItem;
