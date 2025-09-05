import axios from 'axios';
import React, { useState } from 'react'

function Register() {
    const[name, setName]= useState('');
    const[email, setEmail] = useState('');
    const[password, setPassword]=useState('');

    async function HandleRegister() {
        const data={name, email, password};

        try {
        const res = await axios.post("http://localhost:5290/api/User/register", data) 
        console.log(res.data)   
        } catch (error) {
            
        }
    }
  return (
    <div>
      <input type="text" placeholder='name' value={name} onChange={(e)=>setName(e.target.value) } />
      <input type="text" placeholder='email' value={email} onChange={(e)=>setEmail(e.target.value) } />
      <input type="text" placeholder='password' value={password} onChange={(e)=>setPassword(e.target.value) } />
      <button onClick={HandleRegister}>submit</button>
    </div>
  )
}

export default Register
