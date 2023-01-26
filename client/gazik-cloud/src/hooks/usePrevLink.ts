import { useLocation } from "react-router-dom";

export const usePrevLinks = (): { name: string; path: string }[] => {
  const { pathname } = useLocation();
  const arrPathnames = pathname.split("/");
  const arrLinks = [];
  for (let i = 1; i < arrPathnames.length - 1; i++) {
    let nameLink = arrPathnames[i];
    if (i === 1) nameLink = nameLink[0].toUpperCase() + nameLink.slice(1);

    arrLinks.push({
      name: nameLink,
      path: arrPathnames.slice(0, i + 1).join("/"),
    });
  }
  return arrLinks;
};
