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

        public FileController(IFileProcessingService service, IUnitOfWork unitOfWork)
        {
            this.service = service ?? throw new ArgumentException(nameof(service));
            this.unitOfWork = unitOfWork ?? throw new ArgumentException(nameof(unitOfWork));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UplaodFile(IFormFile file)
        {
            if (file == null)
            {
                return this.BadRequest("No file uplaoded!");
            }

            if (file.Length == 0)
            {
                return BadRequest("Empty file!");
            }

            try
            {
                //TODO:

                this.unitOfWork.SaveChanges();

                return this.Ok("File uplaoded successfuly!");
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
