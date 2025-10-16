using System.Threading.Tasks;

namespace myapp.Services
{
    public interface IBackgroundJobService
    {
        Task ProcessDocumentAsync(int documentId, byte[] fileContent, string mimeType);
    }
}
