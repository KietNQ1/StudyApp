using Google.Cloud.AIPlatform.V1;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace myapp.Services
{
    public class VertexAIService
    {
        private readonly PredictionServiceClient _predictionServiceClient;
        private readonly string _textGenerationEndpoint;
        private readonly string _embeddingEndpoint;

        public VertexAIService(IConfiguration configuration)
        {
            _predictionServiceClient = PredictionServiceClient.Create();
            var projectId = configuration["GoogleVertexAI:ProjectId"] ?? throw new InvalidOperationException("ProjectId is not configured.");
            var location = configuration["GoogleVertexAI:Location"] ?? throw new InvalidOperationException("Location is not configured.");
            var textGenerationModel = configuration["GoogleVertexAI:Model"] ?? throw new InvalidOperationException("Model is not configured.");
            var embeddingModel = configuration["GoogleVertexAI:EmbeddingModel"] ?? throw new InvalidOperationException("EmbeddingModel is not configured.");

            _textGenerationEndpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{textGenerationModel}";
            _embeddingEndpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{embeddingModel}";
        }
        
        public async Task<string> PredictTextAsync(string prompt, double temperature = 0.7, int maxTokens = 2048)
        {
            var generateContentRequest = new GenerateContentRequest
            {
                Model = _textGenerationEndpoint,
                Contents =
                {
                    new Content
                    {
                        Role = "user",
                        Parts = { new Part { Text = prompt } }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = (float)temperature,
                    MaxOutputTokens = maxTokens
                }
            };
        
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(generateContentRequest);
            
            if (response.Candidates.Any() && response.Candidates.First().Content.Parts.Any())
            {
                return response.Candidates.First().Content.Parts.First().Text;
            }

            return "No response from AI.";
        }


        public async Task<string> ChatWithDocument(string userMessage, string documentContext)
        {
            var fullPrompt = $"Context: {documentContext}\n\nQuestion: {userMessage}\n\nAnswer based on the context provided. If the answer is not in the context, state that you don't know.";
            return await PredictTextAsync(fullPrompt, 0.2);
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

            var response = await _predictionServiceClient.PredictAsync(predictRequest);
            if (response.Predictions.Any() && response.Predictions[0].StructValue.Fields.TryGetValue("embeddings", out var embeddingsValue))
            {
                if (embeddingsValue.StructValue.Fields.TryGetValue("values", out var valuesList))
                {
                    return valuesList.ListValue.Values.Select(v => (float)v.NumberValue).ToArray();
                }
            }
            return System.Array.Empty<float>();
        }
    }
}
