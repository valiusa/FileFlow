using Data;
using Entities.DataModels;
using FileFlowServer.Controllers;
using Interfaces;
using Interfaces.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace FileFlowServer.Test
{
    public class FileControllerTests
    {
        private readonly FileController controller;
        private readonly Mock<IFileProcessingService> mockFileProcessingService;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;

        public FileControllerTests()
        {
            // Arrange
            this.mockFileProcessingService = new Mock<IFileProcessingService>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();

            this.controller = new FileController(mockFileProcessingService.Object, mockUnitOfWork.Object);
        }

        [Fact]
        public void UploadFiles_NoFiles_ReturnsBadRequest()
        {
            // Arrange
            List<IFormFile> files = null;

            // Act
            var result = this.controller.UploadFiles(files);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No files uploaded!", badRequestResult.Value);
        }

        [Fact]
        public void UploadFiles_ValidFiles_ReturnsOk()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("testfile.txt");

            var files = new List<IFormFile> { mockFile.Object };
            var fileName = "testfile";
            var fileExtension = ".txt";

            var existingFiles = new List<FileStorage>().AsQueryable();  // Simulate no existing files

            // Mocking the IQueryable return
            this.mockFileProcessingService.Setup(s => s.GetAllFiles())
                .Returns(existingFiles);  // Return an IQueryable

            this.mockFileProcessingService.Setup(s => s.UploadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            this.mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = this.controller.UploadFiles(files);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Files uploaded successfully!", okResult.Value);
        }

        [Fact]
        public void GetFiles_ReturnsOkWithFiles()
        {
            // Arrange
            var files = new List<FileStorage>
        {
            new FileStorage { Name = "testfile", Extension = ".txt" }
        }.AsQueryable();  // Return as IQueryable

            this.mockFileProcessingService.Setup(s => s.GetAllFiles()).Returns(files);

            // Act
            var result = this.controller.GetFiles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<FileStorage>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("testfile", returnValue[0].Name);
        }

        [Fact]
        public void DeleteFile_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            this.mockFileProcessingService.Setup(s => s.DeleteFile(It.IsAny<int>()))
                .Throws(new FileNotFoundException("File not found"));

            // Act
            var result = this.controller.DeleteFile(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("File not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public void DeleteFile_ValidFile_ReturnsOk()
        {
            // Arrange
            this.mockFileProcessingService.Setup(s => s.DeleteFile(It.IsAny<int>()));
            this.mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = this.controller.DeleteFile(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File deleted successfully.", okResult.Value);
        }
    }
}