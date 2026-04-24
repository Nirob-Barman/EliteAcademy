using EliteAcademy.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace EliteAcademy.Infrastructure.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;

        public LocalFileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadFileAsync(Stream content, string fileName, string folder)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException("File content is empty.");

            var safeFileName = SanitizeFileName(fileName);
            var folderPath = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, safeFileName);
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await content.CopyToAsync(fileStream);

            return $"/{folder}/{safeFileName}".Replace("\\", "/");
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.CompletedTask;

            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars));
        }
    }
}
