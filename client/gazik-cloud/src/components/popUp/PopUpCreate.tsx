import { useState } from "react";
import { useLocation } from "react-router-dom";
import { PopUpCreateProps } from "../../interfaces/PopUpCreateProps";
import { UserStorageItem } from "../../store/interfaces/IUserStorage";
import { useCreateFolderMutation } from "../../store/userStorageApi";
import Blur from "../effects/Blur";
import Button from "../UI/button/Button";
import TextInput from "../UI/input/TextInput";
import PopUp from "./PopUp";

const PopUpCreate: React.FC<PopUpCreateProps> = ({ display, setDisplay }) => {
  const [nameFolder, setNameFolder] = useState<string>("New folder");
  const [createFolder] = useCreateFolderMutation();
  const { pathname } = useLocation();

  const writeNameFolder = (e: any): void => {
    setNameFolder(e.target.value);
  };

  const handleCreateFolder = async () => {
    if (!nameFolder)
      await createFolder({
        dirName: "new folder",
        destinationId: "0001418c-29b0-4c4d-9a9c-3fe6b2d475dc",
      });
    else {
      await createFolder({
        dirName: nameFolder,
        destinationId: "0001418c-29b0-4c4d-9a9c-3fe6b2d475dc",
      });
    }
    setDisplay(false);
  };

  return (
    <>
      <PopUp close={setDisplay} display={display} variant="create">
        <div className="popup-create-container">
          <h2 className="title-create-folder-name">Specify folder name</h2>
          <TextInput
            type="text"
            id="input-create"
            // placeholder="New folder"
            onChange={writeNameFolder}
            value={nameFolder}
          />
          <div className="btn-create-folder-container">
            <Button onClick={handleCreateFolder} color="yellow">
              Save
            </Button>
          </div>
        </div>
      </PopUp>
      <Blur display={display} onClick={() => setDisplay(false)} />
    </>
  );
};

export default PopUpCreate;
