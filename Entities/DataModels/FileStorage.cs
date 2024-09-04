using System.ComponentModel.DataAnnotations;

namespace Entities.DataModels
{
    public class FileStorage
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
