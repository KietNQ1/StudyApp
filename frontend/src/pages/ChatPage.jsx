import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { authFetch } from '../utils/authFetch';

function ChatPage() {
    const [connection, setConnection] = useState(null);
    const [messages, setMessages] = useState([]);
    const [inputMessage, setInputMessage] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const { sessionId } = useParams();
    const chatContainerRef = useRef(null); // Ref for the message container div

    // Fetch initial chat history
    useEffect(() => {
        const fetchChatHistory = async () => {
            if (!sessionId) return;
            setLoading(true);
            try {
                const sessionData = await authFetch(`/api/ChatSessions/${sessionId}`);
                setMessages(sessionData.chatMessages || []);
            } catch (err) {
                setError(`Failed to load chat history: ${err.message}`);
            } finally {
                setLoading(false);
            }
        };
        fetchChatHistory();
    }, [sessionId]);

    // Set up and manage SignalR connection
    useEffect(() => {
        const token = localStorage.getItem('jwt_token');
        if (!token) {
            window.location.href = '/login';
            return;
        }

        const newConnection = new HubConnectionBuilder()
            .withUrl("/chathub", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        return () => {
            newConnection.stop();
        };
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('Connected to Chat Hub!');
                    connection.invoke("JoinChatSession", sessionId.toString());

                    connection.on("ReceiveMessage", (message) => {
                        setMessages(prevMessages => [...prevMessages, message]);
                    });
                })
                .catch(e => {
                    console.log('Connection failed: ', e);
                    setError('Real-time connection to the server failed.');
                });
        }
    }, [connection, sessionId]);
    
    // Auto-scroll to bottom when new messages are added
    useEffect(() => {
        if (chatContainerRef.current) {
            chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
        }
    }, [messages]);

    const sendMessage = async (e) => {
        e.preventDefault();
        if (inputMessage.trim() && connection) {
            // 1. Optimistic UI Update: Add user's message immediately
            const userMessage = {
                sessionId: parseInt(sessionId),
                content: inputMessage.trim(),
                role: "user",
                createdAt: new Date().toISOString(), // Temporary timestamp
            };
            setMessages(prevMessages => [...prevMessages, userMessage]);
            setInputMessage('');

            // 2. Send to backend without waiting for the full response
            try {
                await authFetch('/api/ChatMessages', {
                    method: 'POST',
                    body: JSON.stringify({
                        sessionId: parseInt(sessionId),
                        content: userMessage.content,
                        role: "user"
                    }),
                });
                // AI response will be pushed by SignalR, no need to handle it here
            } catch (err) {
                console.error('Failed to send message:', err);
                setError(`Failed to send message: ${err.message}`);
                // Optional: Remove the optimistic message or show an error indicator
            }
        }
    };

    if (loading) return <p>Loading chat...</p>;

    return (
        <div className="p-8 max-w-4xl mx-auto">
            <h1 className="text-4xl font-bold mb-4">Chat Session #{sessionId}</h1>
            <div className="bg-white shadow-md rounded-lg h-[600px] flex flex-col">
                <div ref={chatContainerRef} className="flex-1 p-4 overflow-y-auto">
                    {error && <p className="text-red-500 text-center mb-4">{error}</p>}
                    {messages.length === 0 && !loading && (
                        <div className="text-center text-gray-500">
                            No messages yet. Start the conversation!
                        </div>
                    )}
                    {messages.map((msg, index) => (
                        <div key={index} className={`my-2 p-3 rounded-lg max-w-lg ${
                            msg.role === 'assistant' 
                                ? 'bg-blue-100 text-blue-900' 
                                : 'bg-gray-200 text-gray-900 ml-auto'
                        }`}>
                            <p className="whitespace-pre-wrap">{msg.content}</p>
                            <span className="text-xs text-gray-500 block text-right mt-1">
                                {new Date(msg.createdAt).toLocaleTimeString()}
                            </span>
                        </div>
                    ))}
                </div>

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
