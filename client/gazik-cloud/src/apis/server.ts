import axios from "axios";
import FingerprintJS from "@fingerprintjs/fingerprintjs";

const fpPromise = FingerprintJS.load();
export const baseAxios = axios.create({
  baseURL: "http://galaur.ru",
  params: {},
});

export const signUpPost = async (
  userName: string,
  email: string,
  password: string
) => {
  const response = await baseAxios.post("/api/signup", {
    userName,
    email,
    password,
  });
  console.log(response);
};

export const signInPost = async (email: string, password: string) => {
  const fp = await fpPromise;
  const result = await fp.get();
  const response = await baseAxios.post("/api/login", {
    fingerPrint: 'fdsa',
    // fingerPrint: result.visitorId,
    email: email,
    password: password,
  });
  console.log(result.visitorId);
  localStorage.setItem("token", response.data);

  const response1 = await baseAxios.get("/WeatherForecast", {
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
  });
  console.log(response);
  console.log(response1);
};

export const signOutPost = async () => {
  const response = await baseAxios.post(
    `/api/logout/?token=${localStorage.getItem("token")}`
  );
  console.log(response);
};
