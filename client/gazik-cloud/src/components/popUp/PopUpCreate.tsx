import { useState } from "react";
import { PopUpCreateProps } from "../../interfaces/PopUpCreateProps";
import Blur from "../effects/Blur";
import Button from "../UI/button/Button";
import TextInput from "../UI/input/TextInput";
import PopUp from "./PopUp";

const PopUpCreate: React.FC<PopUpCreateProps> = ({ display, setDisplay }) => {
  const [nameFolder, setNameFolder] = useState<string>("New folder");

  const writeNameFolder = (e: any): void => {
    setNameFolder(e.target.value);
  };

	const createFolder = (): void => {
		if (!nameFolder) console.log('New Folder');
		else console.log(nameFolder);
		setDisplay(false);
	}

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
            <Button onClick={createFolder} color="yellow">
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
