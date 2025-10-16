import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import UploadDocumentForm from '../components/UploadDocumentForm'; // Import the new component

function CourseDetailPage() {
  const [course, setCourse] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { id } = useParams(); // Get course ID from URL

  const fetchCourseDetails = () => {
    if (!id) return;

    setLoading(true);
    fetch(`/api/Courses/${id}`)
      .then(response => {
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }
        return response.json();
      })
      .then(data => {
        setCourse(data);
        setLoading(false);
      })
      .catch(error => {
        setError(error.message);
        setLoading(false);
      });
  };

  useEffect(() => {
    fetchCourseDetails();
  }, [id]);

  const handleDocumentUploaded = (newDocument) => {
    // Add the new document to the list to update the UI
    setCourse(prevCourse => ({
      ...prevCourse,
      documents: [...prevCourse.documents, newDocument],
    }));
  };

  if (loading) {
    return <p>Loading course details...</p>;
  }

  if (error) {
    return <p className="text-red-500">Error: {error}</p>;
  }

  if (!course) {
    return <p>Course not found.</p>;
  }

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-2">{course.title}</h1>
      <p className="text-lg text-gray-600 mb-6">{course.description}</p>
      
      <div className="my-8">
        <UploadDocumentForm courseId={course.id} onDocumentUploaded={handleDocumentUploaded} />
      </div>

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
          <p>No documents found for this course. Upload one to get started!</p>
        )}
      </div>
    </div>
  );
}

export default CourseDetailPage;
