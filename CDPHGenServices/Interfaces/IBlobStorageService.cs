using System.IO;
using System.Threading.Tasks;

namespace CDPHGenServices.Interfaces
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Upload stream to blob storage
        /// </summary>
        /// <param name="fileData">File stream</param>
        /// <param name="strFileName">File name</param>
        /// <param name="fileMimeType">File type</param>
        /// <param name="resetStreamPosition">reset stream position</param>
        /// <returns>Blob Path</returns>
        Task<string> UploadStreamToBlobAsync(Stream fileData, string strFileName, string fileMimeType, bool resetStreamPosition = true);

        /// <summary>
        /// Upload text to blob storage.
        /// </summary>
        /// <param name="fileData">Text input</param>
        /// <param name="strFileName">File Name</param>
        /// <param name="fileMimeType">File Type</param>
        /// <returns>Blob path</returns>
        Task<string> UploadTextToBlobAsync(string fileData, string strFileName, string fileMimeType);

        // <summary>
        // Get stream from blob storage recursively from a container
        // </summary>
        // <param name="containerName">Container Name</parm>
        // <param name="strFileName">File Name</param>
        Task<string> GetBlobAsync(string containerName, string strFileName);

                // <summary>
        // Get stream from blob storage recursively from a container
        // </summary>
        // <param name="containerName">Container Name</parm>
        // <param name="strFileName">File Name</param>
        Task<Stream> GetBlobStreamAsync(string containerName, string strFileName);
    }
}