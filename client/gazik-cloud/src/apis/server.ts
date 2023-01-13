import axios from "axios";

export default axios.create({
    baseURL: 'http://galaur.ru/WeatherForecast/upload',
    params: {
    },
})