using Google.Cloud.DocumentAI.V1;
using System.Threading.Tasks;
using Google.Protobuf;

namespace myapp.Services
{
    public class DocumentProcessorService
    {
        private readonly DocumentProcessorServiceClient _client;
        private readonly string _processorName;

        public DocumentProcessorService(string projectId, string location, string processorId)
        {
            _client = DocumentProcessorServiceClient.Create();
            _processorName = ProcessorName.FromProjectLocationProcessor(projectId, location, processorId).ToString();
        }

        public async Task<string> ExtractTextAsync(byte[] fileContent, string mimeType)
        {
            var rawDocument = new RawDocument
            {
                Content = ByteString.CopyFrom(fileContent),
                MimeType = mimeType // Use the provided MimeType
            };

            var request = new ProcessRequest
            {
                Name = _processorName,
                RawDocument = rawDocument
            };

            var response = await _client.ProcessDocumentAsync(request);
            return response.Document.Text;
        }
    }
}
