namespace ECommerce.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);
    Task<bool> DeleteImageAsync(string publicId);
    string ExtractPublicId(string imageUrl);
}