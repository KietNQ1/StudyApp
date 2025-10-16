using Google.Cloud.AIPlatform.V1;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using System.Linq; // Added for .ToArray()

namespace myapp.Services
{
    public class VertexAIService
    {
        private readonly PredictionServiceClient _client;
        private readonly string _textGenerationEndpoint;
        private readonly string _embeddingEndpoint;

        public VertexAIService(string projectId, string location, string publisher, string textGenerationModel, string embeddingModel)
        {
            _client = PredictionServiceClient.Create();
            _textGenerationEndpoint = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{textGenerationModel}";
            _embeddingEndpoint = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{embeddingModel}";
        }

        public async Task<string> PredictTextAsync(string prompt, double temperature = 0.7, int maxTokens = 1024)
        {
            var predictRequest = new PredictRequest
            {
                Endpoint = _textGenerationEndpoint,
                Instances = { Google.Protobuf.WellKnownTypes.Value.ForStruct(new Struct { Fields = { { "prompt", Google.Protobuf.WellKnownTypes.Value.ForString(prompt) } } }) },
                Parameters = Google.Protobuf.WellKnownTypes.Value.ForStruct(new Struct
                {
                    Fields = {
                        { "temperature", Google.Protobuf.WellKnownTypes.Value.ForNumber(temperature) },
                        { "maxOutputTokens", Google.Protobuf.WellKnownTypes.Value.ForNumber(maxTokens) }
                    }
                })
            };

            var response = await _client.PredictAsync(predictRequest);
            if (response.Predictions.Count > 0 && response.Predictions[0].StructValue.Fields.TryGetValue("content", out var contentValue))
            {
                return contentValue.StringValue;
            }
            return "";
        }

        public async Task<string> ChatWithDocument(string userMessage, string documentContext, double temperature = 0.7, int maxTokens = 1024)
        {
            var fullPrompt = $"Context: {documentContext}\n\nQuestion: {userMessage}\n\nAnswer based on the context provided. If the answer is not in the context, state that you don't know.";
            return await PredictTextAsync(fullPrompt, temperature, maxTokens);
        }

        public async Task<string> GenerateQuizQuestions(string content, string topic, int numberOfQuestions = 5, string questionType = "multiple_choice")
        {
            var prompt = $"Generate {numberOfQuestions} {questionType} questions about the following content and topic.\nContent: {content}\nTopic: {topic}\n\nFormat the output as a JSON array of objects, where each object has \"questionText\", \"options\" (for multiple_choice, array of strings), and \"correctAnswer\" (string or index). For short_answer, only \"questionText\" and \"correctAnswer\".";
            return await PredictTextAsync(prompt);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var predictRequest = new PredictRequest
            {
                Endpoint = _embeddingEndpoint,
                Instances = { Google.Protobuf.WellKnownTypes.Value.ForStruct(new Struct { Fields = { { "content", Google.Protobuf.WellKnownTypes.Value.ForString(text) } } }) },
            };

            var response = await _client.PredictAsync(predictRequest);
            if (response.Predictions.Count > 0 && response.Predictions[0].StructValue.Fields.TryGetValue("embeddings", out var embeddingsValue))
            {
                if (embeddingsValue.StructValue.Fields.TryGetValue("values", out var valuesList))
                {
                    return valuesList.ListValue.Values.Select(v => (float)v.NumberValue).ToArray();
                }
            }
            return Array.Empty<float>();
        }
    }
}
