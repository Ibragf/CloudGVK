import { createSlice } from '@reduxjs/toolkit';
import { UserProfileState } from './interfaces/UserProfileState';

const initialState: UserProfileState = {
    isLoggedIn: true,
};

const userProfileSlice = createSlice({
    name: 'user', 
    initialState, 
    reducers: {
        notLoggedIn(state, action) {
            state.isLoggedIn = !state.isLoggedIn;
        },
    },
});

export const {notLoggedIn} = userProfileSlice.actions;
export default userProfileSlice.reducer;