namespace FinancialManagement.Api.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory);

    bool DeleteFile(string fileUrl);
}
