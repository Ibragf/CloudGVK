import React from 'react';
import classes from './SearchInput.module.css'

const SearchInput: React.FC = () => {
    return <input className={classes.searchInput} type="text" placeholder='Search...'/>
}

export default SearchInput;