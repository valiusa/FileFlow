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

        public FileController(IFileProcessingService service, IUnitOfWork unitOfWork)
        {
            this.service = service ?? throw new ArgumentException(nameof(service));
            this.unitOfWork = unitOfWork ?? throw new ArgumentException(nameof(unitOfWork));

            uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            // Ensure the directory exists
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
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

                    if (!IsUnique(name, extension))
                    {
                        return BadRequest($"File with name '{name}' and extension '{extension}' already exists.");
                    }

                    // Define the file path
                    var filePath = Path.Combine(uploadDirectory, file.FileName);

                    this.service.UplaodFile(name, extension, filePath);

                    if (this.unitOfWork.SaveChanges() > 0)
                    {
                        // Save the file to the directory
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
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
                return Ok(files); // Wrap the result in an Ok response
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Handle exceptions
            }
        }

        [HttpDelete("delete")]
        public IActionResult DeleteFile(int key)
        {
            try
            {
                // Attempt to delete the file
                this.service.DeleteFile(key);
                this.unitOfWork.SaveChanges();

                // Return a success response
                return Ok("File deleted successfully.");
            }
            catch (FileNotFoundException ex)
            {
                // Return a 404 Not Found response if the file was not found
                return NotFound($"File not found: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Return a 500 Internal Server Error response for issues with file deletion
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error for any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private (string name, string extension) GetFileNameAndExtension(string fileName)
        {
            // Split file name and extension
            var parts = fileName.Split('.');
            if (parts.Length < 2)
            {
                return (fileName, string.Empty); // Handle cases without an extension
            }

            // Separate the file name and extension
            var extension = $".{parts.Last()}";
            var name = string.Join('.', parts.Take(parts.Length - 1));

            return (name, extension);
        }

        private bool IsUnique(string name, string extension)
        {
            // Check if the combination of name and extension is unique
            // This should query the database or storage to ensure uniqueness
            var existingFiles = service.GetAllFiles();
            return !existingFiles.Any(file => file.Name == name && file.Extension == extension);
        }
    }
}
