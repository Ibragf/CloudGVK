import Button from "../UI/button/Button";
import { HiPlus } from "react-icons/hi";
import PopUpCreate from "../popUp/PopUpCreate";
import { useState } from "react";


const ButtonCreate: React.FC = () => {
  const [openPopUpCreate, setOpenPopupCreate] = useState<boolean>(false);

  return (
    <>
      <Button
        onClick={() => setOpenPopupCreate(!openPopUpCreate)}
        color="white"
        icon={<HiPlus className="sidebar-btn-svg" />}
      >
        Создать
      </Button>
      <PopUpCreate display={openPopUpCreate} setDisplay={setOpenPopupCreate} />
    </>
  );
};

export default ButtonCreate;
