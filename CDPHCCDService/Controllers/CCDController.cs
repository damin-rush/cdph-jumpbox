using CDPHGenServices;
using CDPHGenServices.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace CDPHCCDService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CCDController : ControllerBase
    {
        private readonly ILogger<CCDController> _logger;
        //private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IBlobStorageService _blobSvc;
        private readonly ICcdToFhireService _ccdConverter;


        public CCDController(ILogger<CCDController> logger, IBlobStorageService blobSvc, ICcdToFhireService CcdConverter, IConfiguration config)
        {
            _config = config;
            _logger = logger;            
            _blobSvc = blobSvc;
            _ccdConverter = CcdConverter;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Post mehtod to upload a 5 XML CCD doucments via mulitpart form and save it to blob storage. Note: files with a total size over 30MB
        /// can not be uploaded.r 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("/UploadCCD")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadCCD([FromForm]List<IFormFile> files)
        {
            var ipAddr = HttpContext?.Connection.RemoteIpAddress.ToString();
            _logger.LogDebug("Uploading CCD: Remote IP Address is: {0}", ipAddr); //Log ip address

            if(files == null || files.Count() == 0)
            {
                _logger.LogDebug("No XML doucments included on post: Remote IP Address is: {0}", ipAddr);
                return BadRequest("No XML documents received.");
            }

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var isValidXml = isValidXML(stream); //Validate XML

                        if (isValidXml == null)
                        {
                            _logger.LogDebug("XML document is not valid: Remote IP Address is: {0}", ipAddr);
                            return BadRequest("XML document is not valid.");
                        }

                        var fileName = $"CCD/{isValidXml}/Archive/{isValidXml}_{DateTime.Now.ToUniversalTime().ToString("MMddyHHmmss")}_{Guid.NewGuid().ToString()}";

                        string contentType = "application/octet-stream";
                        if (file.Headers != null && file.ContentType != null && file.ContentType != "")
                        {
                            contentType = file.ContentType;
                        }

                        var ccdPath = await _blobSvc.UploadStreamToBlobAsync(stream, fileName + ".xml", contentType); //Save upload xml to blob storage=
                        _logger.LogInformation("File {0} saved to blob storage path: {1}", fileName + ".xml", ccdPath);

                        var fhir = _ccdConverter.ConvertCcdToFhir(stream); //Convert CCD to Fhir

                        //post CCD to FHIR server here

                        var fhirPath = await _blobSvc.UploadTextToBlobAsync(fhir, fileName.Replace("Archive", "FHIR") + ".fhir.json", "application/json"); //Save fhir to blob storage=
                        _logger.LogInformation("File {0} saved to blob storage path: {1}", fileName + ".fhir.json", fhirPath);

                    }
                }
                else
                {
                    _logger.LogDebug("No XML doucment included on post: Remote IP Address is: {0}", ipAddr);
                    return BadRequest("Empty XML document received.");
                } 
            }
            
            return Ok("File Upload Successful!");

        }

        private string isValidXML(Stream file)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                //TODo: Add schema validation

                var providerName = doc.GetElementsByTagName("representedOrganization").Item(0)["name"].InnerText;
                var split = providerName.Split(" ");
                
                return split.First();
            }
            catch(Exception)
            {
                
            }

            return null;
        }

    }
}