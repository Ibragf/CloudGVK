import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { UserStorageItem } from "./interfaces/IUserStorage";

export const userStorageApi = createApi({
  reducerPath: "userStorageApi",
  baseQuery: fetchBaseQuery({ baseUrl: "http://galaur.ru" }),
  tagTypes: ["Storage"],
  endpoints: (build) => ({
    fetchStorage: build.query<UserStorageItem[], string>({
      // query: (path) => `${path}`,
      query: (path) => ({
        url: `/api/cloud/elements?directoryId=0001418c-29b0-4c4d-9a9c-3fe6b2d475dc`,
        method: "GET",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }),
      providesTags: () => ["Storage"],
    }),
    // createFolder: build.mutation<UserStorageItem, UserStorageItem>({
    //   query: (folder) => ({
    //     url: `${folder.path}`,
    //     method: "POST",
    //     body: folder,
    //   }),
    //   invalidatesTags: ["Storage"],
    // }),
    createFolder: build.mutation<Iinter, Iinter>({
      query: (folder) => ({
        url: `/api/cloud/add/dir?dirName=${folder.dirName}&destinationId=${folder.destinationId}`,
        method: "GET",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }),
      invalidatesTags: ["Storage"],
    }),
  }),
});

export interface Iinter {
  destinationId: string;
  dirName: string;
}

export const { useFetchStorageQuery, useCreateFolderMutation } = userStorageApi;
