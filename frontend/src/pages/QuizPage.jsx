import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { authFetch } from '../utils/authFetch';

function QuizPage() {
  const [quiz, setQuiz] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { id } = useParams();

  useEffect(() => {
    if (!id) return;
    const fetchQuiz = async () => {
      setLoading(true);
      try {
        const data = await authFetch(`/api/Quizzes/${id}`);
        setQuiz(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchQuiz();
  }, [id]);

  if (loading) return <p>Loading quiz...</p>;
  if (error) return <p className="text-red-500">Error: {error}</p>;
  if (!quiz) return <p>Quiz not found.</p>;

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <h1 className="text-4xl font-bold mb-4">{quiz.title}</h1>
      <div className="space-y-8">
        {quiz.quizQuestions.map((quizQuestion, index) => (
          <div key={quizQuestion.id} className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-xl font-semibold mb-4">
              {index + 1}. {quizQuestion.question.questionText}
            </h2>
            <div className="space-y-2">
              {quizQuestion.question.questionOptions.map(option => (
                <div key={option.id} className="flex items-center">
                  <input
                    type="radio"
                    id={`option-${option.id}`}
                    name={`question-${quizQuestion.question.id}`}
                    className="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                  />
                  <label htmlFor={`option-${option.id}`} className="ml-3 block text-sm font-medium text-gray-700">
                    {option.optionText}
                  </label>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
      <div className="mt-8">
        <button className="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded">
          Submit Quiz
        </button>
      </div>
    </div>
  );
}

export default QuizPage;
