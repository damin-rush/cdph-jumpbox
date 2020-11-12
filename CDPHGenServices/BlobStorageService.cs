using CDPHGenServices.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CDPHGenServices
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _accessKey;
        private readonly string _storageContainerName;
        public BlobStorageService(string accessKey, string storageContainerName)
        {
            this._accessKey = accessKey;
            this._storageContainerName = storageContainerName;
        }

        /// <summary>
        /// Upload stream to blob storage
        /// </summary>
        /// <param name="fileData">File stream</param>
        /// <param name="strFileName">File name</param>
        /// <param name="fileMimeType">File type</param>
        /// <param name="resetStreamPosition">reset stream position</param>
        /// <returns>Blob Path</returns>
        public async Task<string> UploadStreamToBlobAsync(Stream fileData, string strFileName, string fileMimeType, bool resetStreamPosition = true)
        {
            try
            {
                if(resetStreamPosition == true)
                {
                    fileData.Position = 0;
                }

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_accessKey);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off }); //Tunr off public access
                }

                if (strFileName != null && fileData != null)
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(strFileName);
                    cloudBlockBlob.Properties.ContentType = fileMimeType;
                    await cloudBlockBlob.UploadFromStreamAsync(fileData);
                    return cloudBlockBlob.Uri.AbsoluteUri;
                }
                else
                {
                    throw new NullReferenceException("FileName and FileData can not be null");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Upload text to blob storage.
        /// </summary>
        /// <param name="fileData">Text input</param>
        /// <param name="strFileName">File Name</param>
        /// <param name="fileMimeType">File Type</param>
        /// <returns>Blob path</returns>
        public async Task<string> UploadTextToBlobAsync(string fileData, string strFileName, string fileMimeType)
        {
            try
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_accessKey);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off }); //Tunr off public access
                }

                if (strFileName != null && fileData != null)
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(strFileName);
                    cloudBlockBlob.Properties.ContentType = fileMimeType;
                    await cloudBlockBlob.UploadTextAsync(fileData);
                    return cloudBlockBlob.Uri.AbsoluteUri;
                }
                else
                {
                    throw new NullReferenceException("FileName and FileData can not be null");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> GetBlobAsync(string containerName, string strFileName)
        {
            try
            {
                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_accessKey);
                    CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

                    if(!await cloudBlobContainer.ExistsAsync())
                        throw new NullReferenceException("Container Name is not valid");

                    if(strFileName != null)
                    {
                            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(strFileName);
                            return await cloudBlockBlob.DownloadTextAsync();
                    }
                    else
                    {
                        throw new NullReferenceException("File name can not be null");
                    }
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string strFileName)
        {
            try
            {
                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_accessKey);
                    CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

                    if(!await cloudBlobContainer.ExistsAsync())
                        throw new NullReferenceException("Container Name is not valid");

                    if(strFileName != null)
                    {
                            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(strFileName);
                            using(MemoryStream downloadStream = new MemoryStream())
                            {
                                await cloudBlockBlob.DownloadToStreamAsync(downloadStream);
                                return downloadStream;
                            }
                            
                    }
                    else
                    {
                        throw new NullReferenceException("File name can not be null");
                    }
            }catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
