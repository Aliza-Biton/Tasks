import React, { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import service from './service.js';

export const Home = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const user = service.getLoginUser();
    if (user) {
      navigate('/tasks'); // אם המשתמש מחובר, ננתב אותו לדף המשימות
    }
  }, [navigate]);

  return (
    <div id='homeDiv'>
      <h2>!ברוכים הבאים</h2>
      <Link to="/login" className="button login">התחברות</Link>
      <br />
      <Link to="/registr" className="button register">הרשמה</Link>
    </div>
  );
};

export default Home;
