import { configureStore } from "@reduxjs/toolkit";
import { userStorageApi } from "./userStorageApi";
import authReducer from "./userProfileSlice";

export const store = configureStore({
  reducer: {
    authReducer,
    [userStorageApi.reducerPath]: userStorageApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(userStorageApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
