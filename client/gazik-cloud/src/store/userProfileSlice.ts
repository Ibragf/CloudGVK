import { createSlice } from '@reduxjs/toolkit';
import { UserProfileState } from './interfaces/UserProfileState';

const initialState: UserProfileState = {
    isLoggedIn: true,
    userData: {
        name: '', 
        email: '',
        password: '',
    }
};

const userProfileSlice = createSlice({
    name: 'user', 
    initialState, 
    reducers: {
        notLoggedIn(state, action) {
            state.isLoggedIn = !state.isLoggedIn;
        },
        setName(state, action) {
            state.userData.name = action.payload;
        },
        setEmail(state, action) {
            state.userData.email = action.payload;
        },
        setPassword(state, action) {
            state.userData.password = action.payload;
        }
    },
});

export const {notLoggedIn, setEmail, setName, setPassword} = userProfileSlice.actions;
export default userProfileSlice.reducer;