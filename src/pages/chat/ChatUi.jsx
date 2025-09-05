import React, { useState, useRef, useEffect, useMemo } from 'react';
import { Send, Search, Phone, Video, MoreVertical, ArrowLeft, Users, Check, CheckCheck, Loader2 } from 'lucide-react';
import * as signalR from '@microsoft/signalr';

function ChatUi() {
  const [currentView, setCurrentView] = useState('userList'); // 'userList' or 'chat'
  const [selectedUser, setSelectedUser] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [messages, setMessages] = useState({});
  const [inputText, setInputText] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [connectionStatus, setConnectionStatus] = useState('Disconnected');
  const messagesEndRef = useRef(null);
  const connectionRef = useRef(null);

  // Initialize SignalR connection
  useEffect(() => {
    const token = localStorage.getItem('Token');
    setConnectionStatus('Connecting...');

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5290/chathub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.start()
      .then(() => {
        setConnectionStatus('Connected');
        connection.on('ReceiveMessage', (senderId, message, timestamp) => {
          setMessages(prev => ({
            ...prev,
            [senderId]: [
              ...(prev[senderId] || []),
              {
                id: Date.now(),
                text: message,
                sender: 'other',
                timestamp: timestamp || new Date().toLocaleTimeString(),
                status: 'delivered'
              }
            ]
          }));
        });

        connection.on('UserTyping', (userId) => {
          if (userId === selectedUser?.id) {
            setIsTyping(true);
            setTimeout(() => setIsTyping(false), 3000);
          }
        });

        connection.onreconnected(() => setConnectionStatus('Connected'));
        connection.onreconnecting(() => setConnectionStatus('Reconnecting...'));
        connection.onclose(() => setConnectionStatus('Disconnected'));
      })
      .catch(err => {
        console.error('SignalR Connection Error:', err);
        setConnectionStatus('Connection Failed');
      });

    return () => {
      connection.stop();
    };
  }, []);

  // Fetch logged-in users
  useEffect(() => {
    const token = localStorage.getItem('Token');
    setLoading(true);

    fetch('http://localhost:5290/api/User/AllUsers', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.ok ? response.json() : Promise.reject(response.status))
      .then(data => {
        const userList = Array.isArray(data) ? data : data.users || data.user || data.data || [];
        setUsers(userList.map((user, index) => ({
          id: user.id || index + 1,
          name: user.name || user.username || user.fullName || 'Unknown User',
          avatar: user.avatar || user.name?.substring(0, 2).toUpperCase() || 'UN',
          status: user.isOnline ? 'online' : 'offline',
          lastSeen: user.isOnline ? 'Online' : `Last seen ${user.lastSeen || 'recently'}`,
          lastMessage: user.lastMessage || '',
          timestamp: user.lastMessageTime || new Date().toLocaleTimeString(),
          unreadCount: user.unreadCount || 0,
          color: `from-${['pink', 'blue', 'green', 'purple', 'orange', 'red', 'indigo', 'teal'][index % 8]}-500 to-${['rose', 'cyan', 'emerald', 'violet', 'amber', 'pink', 'blue', 'cyan'][index % 8]}-500`
        })));
      })
      .catch(error => {
        console.error('Error fetching users:', error);
        setUsers([]);
      })
      .finally(() => setLoading(false));
  }, []);

  // Fetch chat messages for selected user
  useEffect(() => {
    if (!selectedUser) return;

    const token = localStorage.getItem('Token');
    fetch(`http://localhost:5290/api/Message/send/${selectedUser.id}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.ok ? response.json() : Promise.reject(response.status))
      .then(chatMessages => {
        setMessages(prev => ({
          ...prev,
          [selectedUser.id]: chatMessages.map(msg => ({
            id: msg.id,
            text: msg.content,
            sender: msg.senderId === selectedUser.id ? 'other' : 'user',
            timestamp: new Date(msg.timestamp).toLocaleTimeString(),
            status: msg.status || 'sent'
          }))
        }));
      })
      .catch(error => console.error('Error fetching messages:', error));
  }, [selectedUser]);

  // Send message
  const sendMessage = async () => {
    if (!inputText.trim() || !selectedUser) return;

    const token = localStorage.getItem('Token');
    const optimisticMessage = {
      id: Date.now(),
      text: inputText.trim(),
      sender: 'user',
      timestamp: new Date().toLocaleTimeString(),
      status: 'sending'
    };

    setMessages(prev => ({
      ...prev,
      [selectedUser.id]: [...(prev[selectedUser.id] || []), optimisticMessage]
    }));

    try {
      const response = await fetch('http://localhost:5290/api/Message/send', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          receiverId: selectedUser.id,
          content: inputText.trim()
        })
      });

      if (response.ok) {
        const sentMessage = await response.json();
        setMessages(prev => ({
          ...prev,
          [selectedUser.id]: prev[selectedUser.id].map(msg =>
            msg.id === optimisticMessage.id ? { ...msg, id: sentMessage.id, status: 'sent' } : msg
          )
        }));
        if (connectionRef.current?.state === 'Connected') {
          await connectionRef.current.invoke('SendMessage', selectedUser.id, inputText.trim());
        }
      } else {
        throw new Error('Failed to send message');
      }
    } catch (error) {
      console.error('Error sending message:', error);
      setMessages(prev => ({
        ...prev,
        [selectedUser.id]: prev[selectedUser.id].map(msg =>
          msg.id === optimisticMessage.id ? { ...msg, status: 'failed' } : msg
        )
      }));
    }
    setInputText('');
  };

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Memoized filtered users
  const filteredUsers = useMemo(() =>
    users.filter(user => user.name.toLowerCase().includes(searchTerm.toLowerCase())),
    [users, searchTerm]
  );

  // Reusable Avatar component
  const UserAvatar = ({ user, size = 'w-10 h-10' }) => (
    <div className="relative">
      <div className={`${size} bg-gradient-to-r ${user.color} rounded-full flex items-center justify-center text-white font-semibold`}>
        {user.avatar || user.name.charAt(0).toUpperCase()}
      </div>
      <div className={`absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-slate-900 ${
        user.status === 'online' ? 'bg-green-500' : 'bg-gray-500'
      }`}></div>
    </div>
  );

  // User List View
  if (currentView === 'userList') {
    return (
      <div className="flex flex-col h-screen max-h-[800px] bg-gradient-to-br from-slate-900 to-purple-900">
        <div className="bg-black/20 backdrop-blur-sm border-b border-white/10 p-4">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gradient-to-r from-green-500 to-emerald-500 rounded-full flex items-center justify-center">
                <Users className="w-5 h-5 text-white" />
              </div>
              <div>
                <h2 className="text-white font-semibold text-lg">Chat App</h2>
                <p className="text-gray-300 text-sm">{users.length} users • {connectionStatus}</p>
              </div>
            </div>
            <button
              onClick={() => setUsers([]) && setLoading(true) && useEffect(() => {}, [])}
              className="p-2 bg-white/10 hover:bg-white/20 rounded-full"
              disabled={loading}
            >
              {loading ? <Loader2 className="w-5 h-5 text-white animate-spin" /> : <Search className="w-5 h-5 text-white" />}
            </button>
          </div>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search users..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
              className="w-full bg-white/10 border-white/20 rounded-full py-2 pl-10 pr-4 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
          </div>
        </div>
        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <Loader2 className="w-8 h-8 text-purple-500 animate-spin" />
            </div>
          ) : filteredUsers.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-full text-gray-400">
              <Users className="w-16 h-16 mb-4 opacity-50" />
              <p>No users found</p>
              <button onClick={() => setUsers([]) && setLoading(true) && useEffect(() => {}, [])} className="mt-2 text-purple-400 hover:text-purple-300">
                Refresh
              </button>
            </div>
          ) : (
            filteredUsers.map(user => (
              <div
                key={user.id}
                onClick={() => {
                  setSelectedUser(user);
                  setCurrentView('chat');
                }}
                className="flex items-center gap-3 p-4 hover:bg-white/5 cursor-pointer border-b border-white/5"
              >
                <UserAvatar user={user} />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <h3 className="text-white font-medium truncate">{user.name}</h3>
                    <span className="text-xs text-gray-400">{user.timestamp}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <p className="text-sm text-gray-400 truncate">{user.lastMessage || user.lastSeen}</p>
                    {user.unreadCount > 0 && (
                      <span className="bg-green-500 text-white text-xs rounded-full px-2 py-1">{user.unreadCount}</span>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    );
  }

  // Chat View
  return (
    <div className="flex flex-col h-screen max-h-[800px] bg-gradient-to-br from-slate-900 to-purple-900">
      <div className="bg-black/20 backdrop-blur-sm border-b border-white/10 p-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button onClick={() => setCurrentView('userList')} className="p-2 hover:bg-white/10 rounded-full">
            <ArrowLeft className="w-5 h-5 text-white" />
          </button>
          <UserAvatar user={selectedUser} />
          <div>
            <h3 className="text-white font-semibold">{selectedUser?.name}</h3>
            <p className="text-xs text-gray-400">{selectedUser?.lastSeen} • {connectionStatus}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button className="p-2 hover:bg-white/10 rounded-full">
            <Phone className="w-5 h-5 text-white" />
          </button>
          <button className="p-2 hover:bg-white/10 rounded-full">
            <Video className="w-5 h-5 text-white" />
          </button>
          <button className="p-2 hover:bg-white/10 rounded-full">
            <MoreVertical className="w-5 h-5 text-white" />
          </button>
        </div>
      </div>
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {(messages[selectedUser?.id] || []).map(message => (
          <div key={message.id} className={`flex ${message.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div
              className={`max-w-xs px-4 py-2 rounded-2xl shadow ${
                message.sender === 'user'
                  ? 'bg-gradient-to-r from-green-600 to-green-500 text-white rounded-br-sm'
                  : 'bg-white/10 text-white border-white/20 rounded-bl-sm'
              }`}
            >
              <p className="text-sm">{message.text}</p>
              <div className="flex items-center justify-end gap-1 mt-1">
                <span className="text-xs opacity-70">{message.timestamp}</span>
                {message.sender === 'user' && (
                  <div>
                    {message.status === 'sending' && <Loader2 className="w-3 h-3 animate-spin" />}
                    {message.status === 'sent' && <Check className="w-3 h-3" />}
                    {message.status === 'delivered' && <CheckCheck className="w-3 h-3" />}
                    {message.status === 'read' && <CheckCheck className="w-3 h-3 text-blue-400" />}
                    {message.status === 'failed' && <span className="text-red-400 text-xs">!</span>}
                  </div>
                )}
              </div>
            </div>
          </div>
        ))}
        {isTyping && (
          <div className="flex">
            <div className="max-w-xs px-4 py-2 bg-white/10 rounded-2xl border-white/20">
              <div className="flex gap-1">
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" />
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.1s' }} />
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }} />
              </div>
            </div>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>
      <div className="bg-black/20 backdrop-blur-sm border-t border-white/10 p-4">
        <div className="flex items-center gap3">
          <input
            type="text"
            value={inputText}
            onChange={e => setInputText(e.target.value)}
            onKeyPress={e => e.key === 'Enter' && !e.shiftKey && sendMessage()}
            placeholder="Type a message..."
            className="flex-1 bg-white/10 border-white/20 rounded-full py-3 px-4 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-green-500"
          />
          <button
            onClick={sendMessage}
            disabled={!inputText.trim()}
            className="w-12 h-12 bg-gradient-to-r from-green-600 to-green-500 hover:from-green-500 hover:to-green-400 disabled:opacity-50 rounded-full flex items-center justify-center"
          >
            <Send className="w-5 h-5 text-white" />
          </button>
        </div>
      </div>
    </div>
  );
}

export default ChatUi;