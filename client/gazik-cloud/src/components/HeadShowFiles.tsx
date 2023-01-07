import SelectSort from "./UI/select/select-sort/SelectSort";
import { SelectSortOptions } from "./UI/select/select-sort/SelectSortOptions";
import SelectView from "./UI/select/select-view/SelectView";

const HeadShowFiles = (): JSX.Element => {
  return (
    <div className="head-show-files">
      <div className="name-section">Files</div>
      <section className="select-section">
        <SelectSort options={SelectSortOptions} />
        <SelectView />
      </section>
    </div>
  );
};

export default HeadShowFiles;
