using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;


namespace BulkProcess
{
    class Program
    {
		static void Main(string[] args)
		{
			IConfiguration config = new ConfigurationBuilder()
          	.AddJsonFile("appsettings.json", true, true)
          	.Build();

			var appFHIRConfig = config.GetSection("FHIRConverter");
			string url = appFHIRConfig["Url"]+appFHIRConfig["EndPoint"]+appFHIRConfig["templateName"];

			var appBlogStorageConfig = config.GetSection("ConnectionStrings");
			string accessKey = appBlogStorageConfig["AccessKey"];

			CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(accessKey);
			CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

			CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(config["ContainerName"]);
			try{
				CloudBlobDirectory dir = cloudBlobContainer.GetDirectoryReference(config["DirectoryName"]); //root directory
				//var dirFolders = dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, null, null, null).Result;
				var fileNames = dir.ListBlobs().OfType<CloudBlockBlob>();
				//int fileCount = fileNames.Count();
				int count = 0;

				//Parallel.ForEach(fileNames, async (item) =>
        		foreach(var item in fileNames)
				{	
					CloudBlockBlob blob = (CloudBlockBlob)item;
					//string text;
					var request = new RestRequest(Method.POST);					
					using (var memoryStream = new MemoryStream())
					{
						blob.DownloadToStream(memoryStream);
						int contentLength = (int) blob.Properties.Length;
						memoryStream.Position = 0;

                		byte[] xml = new byte[contentLength];
						memoryStream.Read(xml, 0, contentLength);

						//request.AddFile("files", xml, "test.xml", contentType: "application/octet-stream");
						request.AddParameter("text/plain", xml, ParameterType.RequestBody);
					}															

					RestClient client = new RestClient(url){
					//var client = new RestClient("https://cdphservice.azurewebsites.net/UploadCCD");
						Timeout = -1
					};

					string ccd_fhir = config["FHIRDirectoryName"];
					string jsonFileName = $"{ccd_fhir}/{blob.Name.Substring(blob.Name.LastIndexOf("/") + 1).Replace(".xml", "")}.json";

					CloudBlockBlob uploadBlob = cloudBlobContainer.GetBlockBlobReference(jsonFileName);
					uploadBlob.Properties.ContentType = "application/octet-stream";

					void p(Stream responseStream)
					{
						using (responseStream)
						{
							uploadBlob.UploadFromStream(responseStream);
							//responseStream.CopyTo(writer);
						}
					}
					request.ResponseWriter = p;
					var response = client.Post(request);
					/*if (response.StatusCode != System.Net.HttpStatusCode.OK)
					{
						Console.WriteLine($"Error Could not process blob.");

					}*/
					string ccd_archive = config["ArchiveDirectoryName"];
					string blobName = blob.Uri.Segments.Last();
					CloudBlockBlob archiveBlob = cloudBlobContainer.GetBlockBlobReference($"{ccd_archive}/{blobName}");
					archiveBlob.StartCopy(blob);
					blob.Delete();


					Interlocked.Add(ref count, 1);
					//Console.WriteLine($"{count} of {fileCount} processed");
					Console.WriteLine($"{count} files processed");
        		}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}            
            
		}
    }
}