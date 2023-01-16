import HeaderShowContent from "./HeaderShowContent";
import ItemListClientFiles from "./ItemListClientFiles";
import { ItemListClientFilesProps } from "../../interfaces/ItemListClientFilesProps";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

const ShowFiles = (): JSX.Element => {
  const navigate = useNavigate();
  const [listClientFiles, setListClientFiles] = useState<
    ItemListClientFilesProps[]
  >([
    { type: "file", name: "file.txt" },
    {
      type: "folder",
      name: "New folder 1",
      body: [
        { type: "file", name: "file1.txt" },
        { type: "folder", name: "New folder 2" },
      ],
    },
  ]);

  // const listClientFiles: ItemListClientFilesProps[] = [
  //   { type: "file", name: "file.txt" },
  //   {
  //     type: "folder",
  //     name: "New folder 1",
  //     body: [
  //       { type: "file", name: "file1.txt" },
  //       { type: "folder", name: "New folder 2" },
  //     ],
  //   },
  // ];
  return (
    <>
      <HeaderShowContent page="Files" />

      <div className="list-client-content">
        {listClientFiles.map((el, i) => (
          <ItemListClientFiles
            onClick={() => {
              if (el.type === "folder") navigate(el.name);
            }}
            type={el.type}
            name={el.name}
            key={`${el.name}-${i}`}
          />
        ))}
      </div>
    </>
  );
};

export default ShowFiles;
