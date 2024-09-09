using Entities.DataModels;
using Interfaces;
using Interfaces.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace FileFlowServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileProcessingService service;
        private readonly IUnitOfWork unitOfWork;

        private readonly string uploadDirectory;
        private readonly string duplicatesDirectory;

        public FileController(IFileProcessingService service, IUnitOfWork unitOfWork)
        {
            this.service = service ?? throw new ArgumentException(nameof(service));
            this.unitOfWork = unitOfWork ?? throw new ArgumentException(nameof(unitOfWork));

            uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            duplicatesDirectory = Path.Combine(uploadDirectory, "Duplicates");

            // Ensure the directories exist
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }
            if (!Directory.Exists(duplicatesDirectory))
            {
                Directory.CreateDirectory(duplicatesDirectory);
            }
        }

        [HttpPost("upload")]
        public IActionResult UploadFiles([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded!");
            }

            foreach (var file in files)
            {
                try
                {
                    var (name, extension) = GetFileNameAndExtension(file.FileName);
                    var existingFiles = service.GetAllFiles()
                                              .Select(f => f.Name + "" + f.Extension)
                                              .Where(f => f == file.FileName)
                                              .ToList();

                    // When the file has already been added and there is 1 possible combination with the name and extension
                    if (existingFiles.Count == 1 && !this.CanBeDividedMoreThanOnce(file.FileName))
                    {
                        return Conflict("File has already been uploaded once. No further uploads are allowed.");
                    }

                    // First upload: save in the Files directory
                    if (existingFiles.Count == 0)
                    {
                        this.SaveFile(file, name, extension, uploadDirectory);
                        this.service.UploadFile(name, extension, Path.Combine(uploadDirectory, file.FileName));
                        this.unitOfWork.SaveChanges();
                    }
                    // Second upload: move to Duplicates directory with new name structure
                    else if (existingFiles.Count == 1)
                    {
                        // Change name to test and extension to .prod.txt
                        var newName = name.Split('.').First();
                        var newExtension = "." + string.Join(".", name.Split('.').Skip(1)) + extension;

                        this.SaveFile(file, newName, newExtension, duplicatesDirectory);
                        this.service.UploadFile(newName, newExtension, Path.Combine(duplicatesDirectory, newName + newExtension));
                        this.unitOfWork.SaveChanges();
                    }
                    // Third upload: return error, no further uploads allowed
                    else
                    {
                        return StatusCode(429, "File has already been uploaded twice. No further uploads are allowed.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Ok("Files uploaded successfully!");
        }

        [HttpGet("getfiles")]
        public IActionResult GetFiles()
        {
            try
            {
                var files = this.service.GetAllFiles().ToList();
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public IActionResult DeleteFile(int key)
        {
            try
            {
                this.service.DeleteFile(key);
                this.unitOfWork.SaveChanges();
                return Ok("File deleted successfully.");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound($"File not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        private void SaveFile(IFormFile file, string name, string extension, string directory)
        {
            var filePath = Path.Combine(directory, name + extension);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        private bool CanBeDividedMoreThanOnce(string fileName)
        {
            return fileName.Split('.').Count() > 2; // if file name is test.txt when we split => [test, txt] there is only 1 combiantion, but if it is test.prod.txt => [test, prod, txt] there are multiple combinations
        }

        private (string name, string extension) GetFileNameAndExtension(string fileName)
        {
            var parts = fileName.Split('.');
            if (parts.Length < 2)
            {
                return (fileName, string.Empty); // Handle cases without an extension
            }

            var extension = $".{parts.Last()}";
            var name = string.Join('.', parts.Take(parts.Length - 1));
            return (name, extension);
        }
    }
}
