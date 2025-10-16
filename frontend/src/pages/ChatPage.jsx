import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { authFetch } from '../utils/authFetch'; // Import authFetch

function ChatPage() {
    const [connection, setConnection] = useState(null);
    const [messages, setMessages] = useState([]);
    const [inputMessage, setInputMessage] = useState('');
    const latestMessages = useRef(null);
    latestMessages.current = messages; // Keep a ref to the latest messages

    // Hardcoding session ID for demonstration.
    const sessionId = 1; 

    useEffect(() => {
        const token = localStorage.getItem('jwt_token');
        if (!token) {
            window.location.href = '/login';
            return;
        }

        // --- 1. Set up SignalR Connection with JWT Token ---
        const newConnection = new HubConnectionBuilder()
            .withUrl("/chathub", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            // --- 2. Start the connection ---
            connection.start()
                .then(() => {
                    console.log('Connected to Chat Hub!');
                    connection.invoke("JoinChatSession", sessionId.toString());
                    connection.on("ReceiveMessage", (message) => {
                        const updatedMessages = [...latestMessages.current, message];
                        setMessages(updatedMessages);
                    });
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [connection, sessionId]);

    const sendMessage = async (e) => {
        e.preventDefault();
        if (inputMessage && connection) {
            const messageToSend = {
                sessionId: sessionId,
                content: inputMessage,
                role: "user" // Add the missing 'role' property
            };

            try {
                // --- 5. Send message via HTTP POST using authFetch ---
                const result = await authFetch('/api/ChatMessages', {
                    method: 'POST',
                    body: JSON.stringify(messageToSend),
                });
                
                // Add the user's message to the chat immediately
                setMessages([...messages, result.userMessage]);
                
                setInputMessage(''); // Clear input after sending
            } catch (e) {
                console.log('Failed to send message: ', e);
            }
        }
    };

    return (
        <div className="p-8 max-w-4xl mx-auto">
            <h1 className="text-4xl font-bold mb-4">Chat Session #{sessionId}</h1>
            <div className="bg-white shadow-md rounded-lg h-[600px] flex flex-col">
                {/* Message Display Area */}
                <div className="flex-1 p-4 overflow-y-auto">
                    {messages.map((msg, index) => (
                        <div key={index} className={`my-2 p-3 rounded-lg max-w-lg ${
                            msg.role === 'assistant' 
                                ? 'bg-blue-100 text-blue-900' 
                                : 'bg-gray-200 text-gray-900 ml-auto'
                        }`}>
                            <p>{msg.content}</p>
                            <span className="text-xs text-gray-500 block text-right mt-1">
                                {new Date(msg.createdAt).toLocaleTimeString()}
                            </span>
                        </div>
                    ))}
                </div>

                {/* Message Input Form */}
                <div className="p-4 bg-gray-100 border-t">
                    <form onSubmit={sendMessage} className="flex">
                        <input
                            type="text"
                            value={inputMessage}
                            onChange={e => setInputMessage(e.target.value)}
                            className="flex-1 border rounded-l-lg p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Type your message..."
                        />
                        <button
                            type="submit"
                            className="bg-blue-500 text-white px-4 py-2 rounded-r-lg hover:bg-blue-600"
                        >
                            Send
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default ChatPage;
