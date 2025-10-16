import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import AddCourseForm from '../components/AddCourseForm';
import { authFetch } from '../utils/authFetch'; // Import the new fetch utility

function CoursesPage() {
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchCourses = async () => {
    setLoading(true);
    try {
      const data = await authFetch('/api/Courses');
      setCourses(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCourses();
  }, []);

  const handleCourseAdded = (newCourse) => {
    setCourses(prevCourses => [newCourse, ...prevCourses]);
  };

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-8">Courses</h1>
      
      <AddCourseForm onCourseAdded={handleCourseAdded} />

      <h2 className="text-3xl font-bold mb-4">Your Courses</h2>
      {loading && <p>Loading courses...</p>}
      {error && <p className="text-red-500">Error: {error}</p>}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {courses.map(course => (
          <Link to={`/courses/${course.id}`} key={course.id} className="block bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition-shadow">
            <h2 className="text-2xl font-bold mb-2">{course.title}</h2>
            <p className="text-gray-700">{course.description}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}

export default CoursesPage;
