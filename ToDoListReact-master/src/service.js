import axios from 'axios';
import { jwtDecode } from 'jwt-decode';

axios.defaults.baseURL = process.env.REACT_APP_API_URL;
setAuthorizationBearer()

function saveAccessToken(authResult){
  localStorage.setItem("access_token", authResult.token)
  setAuthorizationBearer()
}

function setAuthorizationBearer(){
  const myToken = localStorage.getItem("access_token")
  if(myToken) 
    axios.defaults.headers.common["Authorization"] = `Bearer ${myToken}`
}

axios.interceptors.response.use(
    response => {
      return response},
    error => {
      if (error.status == 401 && window.location.pathname !== '/login') {
        window.location.href = '/login';  // דרך ישירה להפנות לדף login
      }
      else{
        console.log(`ארעה שגיאה!!! קוד שגיאה: ${error.status} , הודעת שגיאה: ${error.response.massage || error.massage}`)
        return Promise.reject(error);  // תחזיר את השגיאה כדי שהקוד יוכל להמשיך לטפל בה אם צריך
      }
    })
const getUserIdFromToken = (token) => {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
    
      const payload = JSON.parse(jsonPayload);
      
      // שימוש בשם ה-claim שמתאים לטוקן שלך (לדוגמה 'sub' או 'NameIdentifier')
      return JSON.parse(jsonPayload).id;  // כאן אני מניח שה-ID נמצא ב-claim שנקרא 'sub'
    }


export default {
  logout: () => {
    localStorage.setItem("access_token","")
  },

  login: async (userName, password) => {
    await setAuthorizationBearer();  
    const res = await axios.post("/login", {nameUser: userName, password: password});
    
    if (res.data.token) {
        console.log("Token received:", res.data.token);
        saveAccessToken(res.data.token);
    } else {
        console.error("Login failed, no token received");
    }
},
  register: async (userName, password) => {
    var d = 0
    const res = await axios.post("/registr",{id:d,NameUser:userName, password:password})
    console.log("Login Response:", res.data);  
    saveAccessToken(res.data.token)
  },

  getLoginUser: () => {
    const token = localStorage.getItem("access_token")
    if(token){
      const currentTime = Math.floor(Date.now() / 1000); // זמן נוכחי בשניות
      return token.exp > currentTime;
    }
    return false
  },

  getIdUser: () => {
    const token = localStorage.getItem("access_token")
    if(token){
      return jwtDecode(token).id
    }
    return null
  },
  getTasks: async () => {    
    const token = localStorage.getItem("access_token");
    const user = jwtDecode(token).id
    const result = await axios.get(`/tasks?id=${user}`);
     return result.data;
  },

  addTask: async(name)=>{
    const token = localStorage.getItem("access_token");
    const user = getUserIdFromToken(token)
    const result = await axios.post(`/tasks?newTask=${encodeURIComponent(name)}&id=${user}`);
    return result.status;
  },

  setCompleted: async(id, isComplete)=>{
    const result = await axios.put(`/tasks/${id}`)
    return {};
  },

  deleteTask:async(id)=>{
    const result = await axios.delete(`/tasks/${id}`);
    return result.status;
  }
};
