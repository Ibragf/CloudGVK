import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { UserStorageItem } from "./interfaces/IUserStorage";



export const userStorageApi = createApi({
  reducerPath: "userStorageApi",
  baseQuery: fetchBaseQuery({ baseUrl: "http://localhost:5000" }),
  endpoints: (build) => ({
    fetchAllStorage: build.query<UserStorageItem[], string>({
      query: () => `/files`,
    }),
  }),
});

export const { useFetchAllStorageQuery } = userStorageApi;
