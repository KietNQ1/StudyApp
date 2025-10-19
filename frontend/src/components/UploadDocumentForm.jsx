import React, { useState } from 'react';
import { authUploadFetch } from '../utils/authFetch'; // Import the new upload utility

function UploadDocumentForm({ courseId, onDocumentUploaded }) {
  const [file, setFile] = useState(null);
  const [title, setTitle] = useState('');
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file || !title) {
      setError('Please provide a title and select a file.');
      return;
    }
    setSubmitting(true);
    setError(null);

    const formData = new FormData();
    formData.append('courseId', courseId);
    formData.append('title', title);
    formData.append('file', file);

    try {
      const newDocument = await authUploadFetch('/api/Documents', {
        method: 'POST',
        body: formData,
      });
      onDocumentUploaded(newDocument);
      setTitle('');
      setFile(null);
      // Clear the file input visually
      e.target.reset(); 
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md mb-8">
      <h3 className="text-2xl font-bold mb-4">Upload a New Document</h3>
      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="doc-title" className="block text-gray-700 font-bold mb-2">
            Document Title
          </label>
          <input
            type="text"
            id="doc-title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            required
          />
        </div>
        <div className="mb-4">
          <label htmlFor="file-upload" className="block text-gray-700 font-bold mb-2">
            File
          </label>
          <input
            type="file"
            id="file-upload"
            onChange={handleFileChange}
            className="block w-full text-sm text-gray-900 border border-gray-300 rounded-lg cursor-pointer bg-gray-50 focus:outline-none"
            required
          />
        </div>
        <button
          type="submit"
          disabled={submitting}
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline disabled:bg-gray-400"
        >
          {submitting ? 'Uploading...' : 'Upload Document'}
        </button>
        {error && <p className="text-red-500 mt-4">{error}</p>}
      </form>
    </div>
  );
}

export default UploadDocumentForm;
