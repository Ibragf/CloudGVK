import HeaderShowStorage from "./HeaderShowStorage";
import StorageItem from "./StorageItem";
import { useFetchStorageQuery } from "../../store/userStorageApi";
import { useLocation, useNavigate } from "react-router-dom";
import { IoMdArrowBack } from "react-icons/io";

const ShowStorage = (): JSX.Element => {
  const { pathname } = useLocation();
  const { data: storage, isLoading, error } = useFetchStorageQuery(pathname);
  console.log(storage);
  const navigate = useNavigate();

  return (
    <>
      <HeaderShowStorage />

      {pathname !== "/files" ? (
        <div className="current-folder">
          <IoMdArrowBack
            className="current-folder-svg"
            onClick={() => navigate(-1)}
          />
          {pathname.split("/")[pathname.split("/").length - 1]}
        </div>
      ) : null}

      {isLoading ? (
        <div>Loading...</div>
      ) : error ? (
        <div>Not found...</div>
      ) : (
        <div className="storage-list">
          {storage &&
            storage.map((item, i) => (
              <StorageItem key={item.id} storageItem={item} />
            ))}
        </div>
      )}
    </>
  );
};

export default ShowStorage;
