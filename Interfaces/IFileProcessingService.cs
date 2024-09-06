using Entities.DataModels;

namespace Interfaces
{
    public interface IFileProcessingService
    {
        void UplaodFile(string name, string extension,  string path);
        void DeleteFile(int id);

        IQueryable<FileStorage> GetAllFiles();
    }
}
