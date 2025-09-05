import React from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Login from './pages/auth/login'
import Register from './pages/auth/register'
import ChatUi from './pages/chat/ChatUi'

function App() {
  return (
   <BrowserRouter>
     <Routes>
       <Route path="/" element={<Login />} />
      <Route path='/register' element={<Register/>}/>
      <Route path='/chat' element={<ChatUi/>}/>
     </Routes>
   </BrowserRouter>
  )
}

export default App
