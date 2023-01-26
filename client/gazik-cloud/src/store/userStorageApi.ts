import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { UserStorageItem } from "./interfaces/IUserStorage";

export const userStorageApi = createApi({
  reducerPath: "userStorageApi",
  baseQuery: fetchBaseQuery({ baseUrl: "http://localhost:5000" }),
  tagTypes: ["Storage"],
  endpoints: (build) => ({
    fetchStorage: build.query<UserStorageItem[], string>({
      query: (path) => `${path}`,
      providesTags: () => ["Storage"],
    }),
    createFolder: build.mutation<UserStorageItem, UserStorageItem>({
      query: (folder) => ({
        url: `${folder.path}`,
        method: "POST",
        body: folder,
      }),
      invalidatesTags: ["Storage"],
    }),
  }),
});

export const { useFetchStorageQuery, useCreateFolderMutation } = userStorageApi;
