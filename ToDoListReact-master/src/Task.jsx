import React, { useEffect, useState } from 'react';
import service from './service.js';
import { useNavigate } from 'react-router-dom';

export const Tasks = () => {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("access_token");
    if (!token) {
      navigate('/login');  // אם אין טוקן תקף, ננתב להתחברות
    } else {
      getTodos();
    }
  }, []);

  async function logout() {
       localStorage.removeItem("access_token");
       navigate('/'); 
     }

  async function getTodos() {
    const todos = await service.getTasks();
    setTodos(todos);
  }

  async function createTodo(e) {
    if (!newTodo.trim()) {
      alert("המשימה לא יכולה להיות ריקה!");
      return;  // לא שולחים את הבקשה אם השדה ריק
    }
    e.preventDefault();
    await service.addTask(newTodo);
    setNewTodo("");//clear input
    await getTodos();//refresh tasks list (in order to see the new one)
  }

  async function updateCompleted(todo, isComplete) {
    await service.setCompleted(todo.id, isComplete);
    await getTodos();//refresh tasks list (in order to see the updated one)
  }

  async function deleteTodo(id) {
    await service.deleteTask(id);
    await getTodos();//refresh tasks list
  }

  useEffect(() => {
    getTodos();
  }, []);

  return (
    <div>
    <section className="todoapp">
      <header className="header">
        <h1 id='myTask'>המשימות שלי</h1>
        <form onSubmit={createTodo}>
          <input className="new-todo" placeholder="הכנסת משימה חדשה" value={newTodo} onChange={(e) => setNewTodo(e.target.value)} />
        </form>

      </header>
      <section className="main" style={{ display: "block" }}>
        <ul className="todo-list">
          {todos.map(todo => {
            return (
              <li className={todo.isComplete ? "completed" : ""} key={todo.id}>
                <div className="view">
                  <input className="toggle" type="checkbox" defaultChecked={todo.isComplete} onChange={(e) => updateCompleted(todo, e.target.checked)} />
                  <label>{todo.name}</label>
                  <button className="destroy" onClick={() => deleteTodo(todo.id)}></button>
                </div>
              </li>
            );
          })}
        </ul>
      </section>

    </section >
    <span id='sp'>
          <button onClick={logout} id='logout'>התנתקות</button>
        </span>
    </div>
  );
}

export default Tasks;




