namespace FileDataExtractService.Implementation
{
    using Azure.Storage.Blobs;
    using FileDataExtractService.Interface;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class BlobWrapper : IBlobWrapper
    {
        public BlobWrapper()
        {
		}

		public async Task<List<string>> GetBlobFileListAsync(string storageConnectionString, string containerName)
		{
			try
			{
				// Get a reference to a container named "sample-container" and then create it
				BlobContainerClient blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
				blobContainerClient.CreateIfNotExists();
				// List all blobs in the container
				var blobItems = blobContainerClient.GetBlobs();

				// Extract the URI of the files into a new list
				List<string> fileUris = new List<string>();
				foreach (var blobItem in blobItems)
				{
					fileUris.Add(blobItem.Name);
				}
				return fileUris;
			}
			catch (Exception ex)
			{
				// Note: When using ASP.NET Core Web Apps, to output to streaming logs, use ILogger rather than System.Diagnostics
				return null;         // or throw e; if you want to bubble the exception up to the caller
			}
		}

		/// <summary>
		/// Get file content.
		/// </summary>
		/// <param name="containerName">The container.</param>
		/// <param name="fileName">The fileName.</param>
		/// <param name="storageConnectionString">The connectionString.</param>
		/// <returns></returns>
		public async Task<List<string>> GetFile(string containerName, string fileName, string storageConnectionString)
        {
			var fileLine = new List<string>();
			try
			{
				BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
				BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
				BlobClient blobClient = containerClient.GetBlobClient(fileName);
				if (await blobClient.ExistsAsync())
				{
					var response = await blobClient.DownloadAsync();
					using (var streamReader = new StreamReader(response.Value.Content))
					{
						while (!streamReader.EndOfStream)
						{
							var line = await streamReader.ReadLineAsync();
							fileLine.Add(line);
						}
					}
				}
			}
			catch (Exception ex)
            {
				return null;
            }

			return fileLine;
		}


		/// <summary>
		/// Get file content.
		/// </summary>
		/// <param name="containerName">The container.</param>
		/// <param name="fileName">The fileName.</param>
		/// <param name="storageConnectionString">The connectionString.</param>
		/// <param name="archiveContainer">The archiveContainer.</param>
		/// <returns></returns>
		public async Task<bool> MoveFile(string containerName, string fileName, string storageConnectionString, string archiveContainer)
		{
			var fileLine = new List<string>();
			try
			{
				//Get source and destination azure storage account connection string from app.config
				CloudStorageAccount sourceStorageConnectionString = CloudStorageAccount.Parse(storageConnectionString);
				CloudStorageAccount destinationStorageConnectionString = CloudStorageAccount.Parse(storageConnectionString);
				CloudBlobClient sourceCloudBlobClient = sourceStorageConnectionString.CreateCloudBlobClient();
				CloudBlobClient targetCloudBlobClient = destinationStorageConnectionString.CreateCloudBlobClient();
				//Get source and destination container name from app.config
				CloudBlobContainer sourceContainer = sourceCloudBlobClient.GetContainerReference(containerName);
				CloudBlobContainer destinationContainer = targetCloudBlobClient.GetContainerReference(archiveContainer);
				await destinationContainer.CreateIfNotExistsAsync();
				CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(fileName);
				CloudBlockBlob targetBlob = destinationContainer.GetBlockBlobReference(fileName);
				//copy blob from source to destination
				await targetBlob.StartCopyAsync(sourceBlob).ConfigureAwait(false);

				//check if blob exists into destination container
				if (await targetBlob.ExistsAsync())
				{
					//Delete blob from source container
					await sourceBlob.DeleteIfExistsAsync();
				}

			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Save report.
		/// </summary>
		/// <param name="fileName">The fileName.</param>
		/// <param name="fileContent">The report content.</param>
		/// <param name="containerName">The container.</param>
		/// <param name="storageConnectionString">The connectionString.</param>
		/// <returns></returns>
		public async Task SaveReport(string fileName, string fileContent, string containerName, string storageConnectionString)
        {
			try
			{
				BlobContainerClient blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
				await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
				using (MemoryStream stream = new MemoryStream())
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.Write(fileContent);
						writer.Flush();
						stream.Position = 0;
						fileName = fileName + "-" + Regex.Replace(DateTime.UtcNow.ToString(), "[^a-zA-Z0-9% ._]", string.Empty) + ".txt";
						await blobContainerClient.UploadBlobAsync(fileName, stream).ConfigureAwait(false);
					}					
				}
			}
			catch(Exception ex)
            {
				throw ex;
            }
		}
	}
}
