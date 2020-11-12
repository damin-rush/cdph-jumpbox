using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CCDConsumer
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([BlobTrigger("ccdconsumertest/{name}", Connection = "")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            try
            {
                #region "Send To Azure"
                using (var ms = new MemoryStream())
                {
                    myBlob.Position = 0;
                    myBlob.CopyTo(ms);

                    var client = new RestClient("https://cdphservice.azurewebsites.net/UploadCCD");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "multipart/form-data");
                    //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddFile("files", ms.ToArray(), "test.xml", contentType: "application/octet-stream");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        log.LogError($"Could not upload ccd Name:{name}");

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {

                log.LogError($"Could not process XML {name}.", ex);
            }
        }
    }
}
