import React, { useState } from 'react';
import { authFetch } from '../utils/authFetch';

function GenerateQuizForm({ course, onQuizGenerated }) {
  const [documentId, setDocumentId] = useState('');
  const [title, setTitle] = useState('');
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!documentId || !title) {
      setError('Please select a document and provide a title.');
      return;
    }
    setSubmitting(true);
    setError(null);
    
    // In a real app, you'd get the current user's ID from an auth context
    const currentUserId = 1; 

    try {
      const newQuiz = await authFetch('/api/Quizzes/Generate', {
        method: 'POST',
        body: JSON.stringify({
          courseId: course.id,
          documentId: parseInt(documentId),
          title: title,
          createdByUserId: currentUserId,
          numberOfQuestions: 5,
        }),
      });
      onQuizGenerated(newQuiz);
      setTitle('');
      setDocumentId('');
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md mb-8">
      <h3 className="text-2xl font-bold mb-4">Generate a New Quiz</h3>
      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="quiz-title" className="block text-gray-700 font-bold mb-2">
            Quiz Title
          </label>
          <input
            type="text"
            id="quiz-title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            required
          />
        </div>
        <div className="mb-4">
          <label htmlFor="document-select" className="block text-gray-700 font-bold mb-2">
            Generate from Document
          </label>
          <select
            id="document-select"
            value={documentId}
            onChange={(e) => setDocumentId(e.target.value)}
            className="shadow border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            required
          >
            <option value="" disabled>Select a document</option>
            {course.documents.map(doc => (
              <option key={doc.id} value={doc.id}>{doc.title}</option>
            ))}
          </select>
        </div>
        <button
          type="submit"
          disabled={submitting || course.documents.length === 0}
          className="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline disabled:bg-gray-400"
        >
          {submitting ? 'Generating...' : 'Generate Quiz'}
        </button>
        {course.documents.length === 0 && <p className="text-sm text-gray-500 mt-2">Upload a document to generate a quiz.</p>}
        {error && <p className="text-red-500 mt-4">{error}</p>}
      </form>
    </div>
  );
}

export default GenerateQuizForm;
