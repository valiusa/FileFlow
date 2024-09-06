using Entities.DataModels;
using Interfaces;
using Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IRepository<FileStorage> repository;

        public FileProcessingService(IRepository<FileStorage> repository)
        {
            this.repository = repository ?? throw new ArgumentException(nameof(repository));
        }

        public void DeleteFile(int id)
        {
            var file = this.repository.Get(id);

            if (file == null)
            {
                throw new ArgumentException("File not found");
            }

            // Delete the file from the file system
            try
            {
                // Check if the file exists before attempting to delete
                if (File.Exists(Path.Combine(file.Path)))
                {
                    File.Delete(file.Path);
                }
                else
                {
                    throw new FileNotFoundException("File not found on the file system");
                }

                this.repository.Delete(id);
            }
            catch (Exception ex)
            {
                // Log the error and optionally rethrow or handle it
                throw new InvalidOperationException("Error deleting file from the file system", ex);
            }
        }

        public IQueryable<FileStorage> GetAllFiles()
        {
            return this.repository.Queryable().AsNoTracking();
        }

        public void UplaodFile(string name, string extension, string path)
        {
            var newFile = new FileStorage();
            newFile.Name = name;
            newFile.Extension = extension;
            newFile.Path = path;
            newFile.CreatedOn = DateTime.Now;

            this.repository.Add(newFile);
        }
    }
}
