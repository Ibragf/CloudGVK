import { useNavigate } from "react-router-dom";
import { usePrevLinks } from "../../hooks/usePrevLink";
import { MdKeyboardArrowRight } from "react-icons/md";

const FolderLinks = (): JSX.Element => {
  const navigation = useNavigate();
  const prevLiks = usePrevLinks();

  return prevLiks.length ? (
    <div className="folder-links">
      {prevLiks.map((link, i) => (
        <div className="container-folder-link" key={`${link.name}-${i}`}>
          <div
            className="name-folder-link"
            onClick={() => navigation(link.path)}
          >
            {link.name}
          </div>
          <MdKeyboardArrowRight className="name-page-svg" />
        </div>
      ))}
    </div>
  ) : (
    <div className="name-page">Files</div>
  );
};

export default FolderLinks;
