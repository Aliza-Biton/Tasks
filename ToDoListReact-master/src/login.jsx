import service from './service.js'; // מייבא את השירות שלך
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';


export const Login = () => {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  //בדיקה האם המשתמש מחובר
    useEffect(() => {
      const user = service.getLoginUser();
      if (user) {
        navigate('/tasks'); 
      }
    }, []);
    
//פונקציית החיבור
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await service.login(userName, password);
      navigate('/tasks'); 
    } catch (e) {
      alert(e);
    }
  }

  return (
    <div id='loginDiv'>
      <h2>התחברות</h2>
      <form onSubmit={handleSubmit}>
        <input className='name'
          type="text" 
          placeholder="שם משתמש" 
          value={userName} 
          onChange={(e) => setUserName(e.target.value)} 
        />
        <input className='pass'
          type="password" 
          placeholder="סיסמה" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
        />
        <button type="submit">התחבר</button>
      </form>
      {error && <p>{error}</p>}
    </div>
  );
}

export default Login;
