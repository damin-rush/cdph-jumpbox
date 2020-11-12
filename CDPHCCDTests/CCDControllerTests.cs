using CDPHCCDService.Controllers;
using CDPHGenServices;
using CDPHGenServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Web.Http.Results;

namespace CDPHCCDTests
{
    [TestClass]
    public class CCDControllerTests
    {

        private static Mock<ILogger<CCDController>> mockLogger;
        private static Mock<IBlobStorageService> bloblSvc;
        private static Mock<IConfiguration> mockConfiguration;
        private static Mock<ICcdToFhireService> mockFhireConverter;
        private static CCDController controller;

        [AssemblyInitialize()]
        public static void MyTestInitialize(TestContext testContext)
        {
            mockLogger = new Mock<ILogger<CCDController>>();
            bloblSvc = new Mock<IBlobStorageService>();
            mockConfiguration = new Mock<IConfiguration>();
            mockFhireConverter = new Mock<ICcdToFhireService>();
            controller = new CCDController(mockLogger.Object, bloblSvc.Object, mockFhireConverter.Object, mockConfiguration.Object);
        }

        [TestMethod]
        public void UploadCCD_ShouldReturnOkReuslt_WhenValidXMLFileIsProvided()
        {
            //Arange
            var fileStream = File.Open("./TestFiles/ValidCCD.xml", FileMode.Open);

            FormFile formFile = new FormFile(fileStream, 0, fileStream.Length, "File", "ValidCCD.xml");
            FormFile formFile2 = new FormFile(fileStream, 0, fileStream.Length, "File", "ValidCCD.xml");
            
            List<IFormFile> files = new List<IFormFile>()
            {
                formFile,
                formFile2
            };
            //Act
            var actionResults = controller.UploadCCD(files);

            //Assert
            Assert.IsTrue(actionResults.Result is OkObjectResult);
        }

        [TestMethod]
        public void UploadCCD_ShouldReturnBadRequestReuslt_WhenValidXMLFileIsInvalid()
        {
            //Arange
            var fileStream = File.Open("./TestFiles/InValidCCD.xml", FileMode.Open);

            FormFile formFile = new FormFile(fileStream, 0, fileStream.Length, "File", "InValidCCD.xml");
            List<IFormFile> files = new List<IFormFile>()
            {
                formFile,
            };
            //Act
            var actionResults = controller.UploadCCD(files);

            //Assert
            Assert.IsTrue(actionResults.Result is BadRequestObjectResult);
        }

        [TestMethod]
        public void UploadCCD_ShouldReturnBadRequestReuslt_WhenNullIsPassedIn()
        {
            //Arange
      
            //Act
            var actionResults = controller.UploadCCD(null);

            //Assert
            Assert.IsTrue(actionResults.Result is BadRequestObjectResult);
        }
    }
}
