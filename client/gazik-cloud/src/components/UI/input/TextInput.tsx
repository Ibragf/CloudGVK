import React from 'react';
import classes from './TextInput.module.css';
import { TextInputProps } from '../../../interfaces/TextInputProps';

const TextInput: React.FC<TextInputProps> = ({type, id, width, ...props}) => {
    return <input {...props} className={classes.textInput} type={type} id={id} style={{width: width}} />
}

export default TextInput;