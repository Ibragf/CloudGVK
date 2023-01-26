import HeaderShowStorage from "./HeaderShowStorage";
import StorageItem from "./StorageItem";
import { useFetchStorageQuery } from "../../store/userStorageApi";
import { useLocation } from "react-router-dom";

const ShowStorage = (): JSX.Element => {
  const { pathname } = useLocation();
  const { data: storage, isLoading, error } = useFetchStorageQuery(pathname);

  return (
    <>
      <HeaderShowStorage />

      {/* Добавить возврат на пред ссылку если locaton !== /files*/}
      {pathname !== "/files" ? (
        <div className="current-folder">{pathname.split("/")[pathname.split("/").length - 1]}</div>
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
