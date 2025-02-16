import React, { useState } from 'react';
import service from './service.js'; // מייבא את השירות שלך
import { useNavigate } from 'react-router-dom';


export const Register = () => {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

//פונקצייתת התחברות
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await service.register(userName, password); 
      navigate('/tasks');
    } catch (e) {
      setError("שגיאה בהרשמה.");
    }
  }

  return (
    <div id='registrDiv'>
      <h2>הרשמה</h2>
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
        <button type="submit">הרשם</button>
      </form>
      {error && <p>{error}</p>}
    </div>
  );
}

export default Register;
