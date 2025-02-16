import { Route, Routes } from "react-router-dom"
import { Register } from "./registr"
import { Login } from "./login"
import { Home } from "./home"
import { Tasks } from "./Task"

export const Routing = () => {
    return <>
    <Routes>
        <Route path="/" element={<Home></Home>}>דף הבית</Route>
        <Route path="home" element={<Home></Home>}>דף הבית</Route>
        <Route path="registr" element={<Register></Register>}></Route>
        <Route path="login" element={<Login></Login>}></Route>
        <Route path="tasks" element={<Tasks></Tasks>}></Route>
    </Routes>
    </>
}