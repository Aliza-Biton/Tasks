import './App.css';
import { BrowserRouter } from 'react-router-dom';
import { Routing } from './routing';


function App() {
  return (
    <div className="App">
      <BrowserRouter>
      <div className='container'>
      <Routing></Routing>
      </div>
      </BrowserRouter>

    </div>
  );
}

export default App;
