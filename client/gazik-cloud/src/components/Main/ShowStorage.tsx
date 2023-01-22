import HeaderShowContent from "./HeaderShowContent";
import StorageItem from "./StorageItem";
import { useFetchAllStorageQuery } from "../../store/userStorageApi";

const ShowStorage = (): JSX.Element => {
  const { data: storage, isLoading } = useFetchAllStorageQuery("");

  return (
    <>
      <HeaderShowContent page="Files" />

      {isLoading ? (
        <div>Loading...</div>
      ) : (
        <div className="show-content-list">
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
