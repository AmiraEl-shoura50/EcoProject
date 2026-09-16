using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Services;

public class CloudinaryService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            Transformation = new Transformation().Width(800).Height(800).Crop("limit") // تحديد أقصى حجم عشان الأداء
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
        {
            throw new Exception($"فشل رفع الصورة: {result.Error.Message}");
        }

        return result.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }

    public string ExtractPublicId(string imageUrl)
    {
        // مثال على الـ URL: https://res.cloudinary.com/xxx/image/upload/v123456/products/abc123.jpg
        // الـ PublicId المطلوب: products/abc123

        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');

        // بناخد كل حاجة بعد "upload/v123456/" ونشيل الـ extension
        var uploadIndex = Array.IndexOf(segments, "upload");
        var relevantSegments = segments.Skip(uploadIndex + 2); // نتخطى "upload" و "v123456"

        var publicIdWithExtension = string.Join("/", relevantSegments);
        return Path.ChangeExtension(publicIdWithExtension, null); // بيشيل الـ .jpg/.png
    }
}