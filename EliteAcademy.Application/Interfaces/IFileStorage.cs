namespace EliteAcademy.Application.Interfaces
{
    public interface IFileStorage
    {
        Task<string> UploadFileAsync(Stream content, string fileName, string folder);
        Task DeleteFileAsync(string filePath);
    }
}
