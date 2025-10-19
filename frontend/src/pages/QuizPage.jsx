import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { authFetch } from '../utils/authFetch';

function QuizPage() {
  const [quiz, setQuiz] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [answers, setAnswers] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState(null);
  const { id } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
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

  const handleOptionChange = (questionId, optionId) => {
    setAnswers(prev => ({
      ...prev,
      [questionId]: optionId,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      // Step 1: Create a new Quiz Attempt to get an attemptId
      const attemptData = await authFetch('/api/QuizAttempts', {
        method: 'POST',
        body: JSON.stringify({ quizId: quiz.id, userId: 1 }), // Hardcoding userId = 1
      });

      // Step 2: Format the answers to match the backend model
      const formattedAnswers = Object.keys(answers).map(questionId => ({
        questionId: parseInt(questionId),
        selectedOptionId: answers[questionId],
      }));

      // Step 3: Submit the formatted answers for grading
      const resultData = await authFetch(`/api/QuizAttempts/${attemptData.id}/Submit`, {
        method: 'POST',
        body: JSON.stringify(formattedAnswers),
      });
      
      setResult(resultData); // Display the final score and results

    } catch (err) {
      setError(`Failed to submit quiz: ${err.message}`);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p>Loading quiz...</p>;
  if (error) return <p className="text-red-500">Error: {error}</p>;
  if (!quiz) return <p>Quiz not found.</p>;

  // Display results if the quiz has been submitted
  if (result) {
    return (
      <div className="p-8 max-w-4xl mx-auto text-center">
        <h1 className="text-4xl font-bold mb-4">Quiz Results</h1>
        <div className="bg-white p-8 rounded-lg shadow-md">
            <p className="text-lg mb-2">You scored:</p>
            <p className="text-6xl font-bold text-blue-600 mb-4">{result.percentage.toFixed(2)}%</p>
            <p className="text-gray-700">({result.score} / {result.totalPoints} points)</p>
            <button 
                onClick={() => navigate('/courses')} 
                className="mt-8 bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            >
                Back to Courses
            </button>
        </div>
      </div>
    );
  }

  // Display the quiz questions
  return (
    <div className="p-8 max-w-4xl mx-auto">
      <h1 className="text-4xl font-bold mb-4">{quiz.title}</h1>
      <form onSubmit={handleSubmit}>
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
                      value={option.id}
                      onChange={() => handleOptionChange(quizQuestion.question.id, option.id)}
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
          <button 
            type="submit"
            disabled={submitting}
            className="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded disabled:bg-gray-400"
          >
            {submitting ? 'Submitting...' : 'Submit Quiz'}
          </button>
        </div>
      </form>
    </div>
  );
}

export default QuizPage;
