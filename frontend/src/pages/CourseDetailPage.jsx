import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import UploadDocumentForm from '../components/UploadDocumentForm';
import GenerateQuizForm from '../components/GenerateQuizForm'; // Import the new component
import { authFetch } from '../utils/authFetch';

function CourseDetailPage() {
  const [course, setCourse] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { id } = useParams();

  const fetchCourseDetails = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const data = await authFetch(`/api/Courses/${id}`);
      setCourse(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCourseDetails();
  }, [id]);

  const handleDocumentUploaded = (newDocument) => {
    setCourse(prevCourse => ({
      ...prevCourse,
      documents: [...prevCourse.documents, newDocument],
    }));
  };
  
  const handleQuizGenerated = (newQuiz) => {
    setCourse(prevCourse => ({
        ...prevCourse,
        quizzes: [...prevCourse.quizzes, newQuiz],
      }));
  };

  if (loading) return <p>Loading course details...</p>;
  if (error) return <p className="text-red-500">Error: {error}</p>;
  if (!course) return <p>Course not found.</p>;

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-2">{course.title}</h1>
      <p className="text-lg text-gray-600 mb-6">{course.description}</p>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        <div>
          <UploadDocumentForm courseId={course.id} onDocumentUploaded={handleDocumentUploaded} />
          <div className="mt-8">
            <h2 className="text-3xl font-bold mb-4">Documents</h2>
            {course.documents && course.documents.length > 0 ? (
              <ul className="space-y-4">
                {course.documents.map(doc => (
                  <li key={doc.id} className="bg-white p-4 rounded-lg shadow-md">
                    <p className="font-semibold">{doc.title}</p>
                    <p className="text-sm text-gray-500">{doc.fileType}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p>No documents found. Upload one to get started!</p>
            )}
          </div>
        </div>
        <div>
          <GenerateQuizForm course={course} onQuizGenerated={handleQuizGenerated} />
          <div className="mt-8">
            <h2 className="text-3xl font-bold mb-4">Quizzes</h2>
            {course.quizzes && course.quizzes.length > 0 ? (
              <ul className="space-y-4">
                {course.quizzes.map(quiz => (
                  <li key={quiz.id} className="bg-white p-4 rounded-lg shadow-md flex justify-between items-center">
                    <p className="font-semibold">{quiz.title}</p>
                    <Link to={`/quiz/${quiz.id}`} className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                      Start Quiz
                    </Link>
                  </li>
                ))}
              </ul>
            ) : (
              <p>No quizzes found. Generate one from a document!</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default CourseDetailPage;
