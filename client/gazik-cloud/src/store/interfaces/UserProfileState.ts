export interface UserProfileState {
    isLoggedIn: boolean,
    userData: {
        name: string, 
        email: string, 
        password: string,
    },
}