import Button from "../UI/button/Button";
import { MdUpload } from "react-icons/md";
import { ChangeEvent, useRef } from "react";
import SparkMD5 from "spark-md5";

const ButtonLoad: React.FC = () => {
  const refLoadFile = useRef<HTMLInputElement>(null);

  // ChangeEvent<HTMLInputElement>
  const downloadFile = (e: any): void => {
    // const file = e.target.files[0];
    // const formData = new FormData();
    var blobSlice = File.prototype.slice,
      file: any = e.target.files[0],
      chunkSize = 2097152, // Read in chunks of 2MB
      chunks = Math.ceil(file.size / chunkSize),
      currentChunk = 0,
      spark = new SparkMD5.ArrayBuffer(),
      fileReader = new FileReader();

    fileReader.onload = function (ev) {
      console.log("read chunk nr", currentChunk + 1, "of", chunks);
      const v: any = ev.target?.result;
      spark.append(v); // Append array buffer
      currentChunk++;

      if (currentChunk < chunks) {
        loadNext();
      } else {
        console.log("finished loading");
        console.info("computed hash", spark.end()); // Compute hash
      }
    };

    fileReader.onerror = function () {
      console.warn("oops, something went wrong.");
    };

    function loadNext() {
      var start = currentChunk * chunkSize,
        end = start + chunkSize >= file.size ? file.size : start + chunkSize;

      fileReader.readAsArrayBuffer(blobSlice.call(file, start, end));
    }

    loadNext();
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
