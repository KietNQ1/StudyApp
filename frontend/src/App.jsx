import { Outlet, Link } from "react-router-dom";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <nav className="bg-white shadow-md">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <div className="flex-shrink-0 flex items-center">
                <Link to="/" className="text-2xl font-bold text-blue-600">Study Assistant</Link>
              </div>
            </div>
            <div className="flex items-center">
              <Link to="/" className="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-200">Home</Link>
              <Link to="/courses" className="ml-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-200">Courses</Link>
              <Link to="/chat" className="ml-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-200">Chat</Link>
            </div>
          </div>
        </div>
      </nav>
      <main>
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

export default App;
