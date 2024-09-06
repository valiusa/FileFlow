using Entities.DataModels;
using Interfaces;
using Interfaces.Repository;

namespace Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IRepository<FileStorage> repository;

        public FileProcessingService(IRepository<FileStorage> repository)
        {
            this.repository = repository ?? throw new ArgumentException(nameof(repository));
        }

        public void UplaodFile(string name, string path)
        {
            var newFile = new FileStorage();
            // TODO:
            newFile.CreatedOn = DateTime.Now;

            this.repository.Add(newFile);
        }
    }
}
