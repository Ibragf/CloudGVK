import Button from "../UI/button/Button";
import { MdUpload } from "react-icons/md";
import { useRef } from "react";

const ButtonLoad: React.FC = () => {
  const refLoadFile = useRef<HTMLInputElement>(null);

  const downloadFile = (e: any): void => {
    const file = e.target.files[0];
    console.log(e);
    const reader = new FileReader();
    reader.readAsText(file);

    // reader.onload = function () {
    //   console.log(reader.result);
    // };

    // reader.onerror = function () {
    //   console.log(reader.error);
    // };

    // const formData = new FormData();
    // console.log(file);
    // formData.append("name", file);
    // console.log(e);
  };

  const handleLoadFile = (): void => {
    if (refLoadFile.current) refLoadFile.current.click();
  };

  return (
    <Button
      onClick={handleLoadFile}
      color="yellow"
      icon={<MdUpload className="sidebar-btn-svg" />}
    >
      <input
        ref={refLoadFile}
        onChange={downloadFile}
        type="file"
        className="dis-none"
        id="file-load"
      />
      Загрузить
    </Button>
  );
};

export default ButtonLoad;
