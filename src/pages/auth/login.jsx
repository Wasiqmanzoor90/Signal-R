// ✅ Fix
import React, { useState } from "react";
import axios from 'axios';

function Login() {
  const[email, setEmail] = useState("");
const[password, setPassword] = useState("");


async function Handlelogin(e){
  const data = {email, password};
  e.preventDefault();
  try {
    const res = await  axios.post('http://localhost:5290/api/User/login',data);
    localStorage.setItem("Token", res.data.token)
console.log(res.data);

  } catch (error) {
    console.log(error);
  }
}
  return (
    
    <div className='login-page'>
      <form onSubmit={Handlelogin}>
      <input type="email" placeholder="Email" value={email} onChange={(e)=> setEmail(e.target.value)} />
      <input type="password" placeholder="Password" value={password} onChange={(e)=> setPassword(e.target.value)}/>
      <button type="submit">submit</button>
      </form>
    </div>
    
  )
}

export default Login