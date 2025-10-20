import React from 'react'
import ReactDOM from 'react-dom/client'
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";
import { GoogleOAuthProvider } from '@react-oauth/google';
import App from './App.jsx'
import HomePage from './pages/HomePage.jsx';
import CoursesPage from './pages/CoursesPage.jsx';
import CourseDetailPage from './pages/CourseDetailPage.jsx';
import ChatPage from './pages/ChatPage.jsx';
import LoginPage from './pages/LoginPage.jsx';
import QuizPage from './pages/QuizPage.jsx';
import './index.css'

const GOOGLE_CLIENT_ID = "264224102507-jd0sl8ig5fq2seuo05keav8n7vanq8uu.apps.googleusercontent.com";

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        path: "/",
        element: <HomePage />,
      },
      {
        path: "courses",
        element: <CoursesPage />,
      },
      {
        path: "courses/:id",
        element: <CourseDetailPage />,
      },
      {
        path: "chat/:sessionId",
        element: <ChatPage />,
      },
      {
        path: "quiz/:id",
        element: <QuizPage />,
      }
    ],
  },
  {
    path: "/login",
    element: <LoginPage />,
  }
]);

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <RouterProvider router={router} />
    </GoogleOAuthProvider>
  </React.StrictMode>,
)
