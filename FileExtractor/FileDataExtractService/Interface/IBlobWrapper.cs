namespace FileDataExtractService.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBlobWrapper
    {
        /// <summary>
        /// Get all the blob from a container and returns the URL of the files.
        /// </summary>
        /// <param name="storageConnectionString">The connectionstring.</param>
        /// <param name="containerName">The container.</param>
        /// <returns></returns>
        Task<List<string>> GetBlobFileListAsync(string storageConnectionString, string containerName);

        /// <summary>
        /// Get file content.
        /// </summary>
        /// <param name="containerName">The container.</param>
        /// <param name="fileName">The fileName.</param>
        /// <param name="storageConnectionString">The connectionString.</param>
        /// <returns></returns>
        Task<List<string>> GetFile(string containerName, string fileName, string storageConnectionString);

        /// <summary>
        /// Save report.
        /// </summary>
        /// <param name="fileName">The fileName.</param>
        /// <param name="fileContent">The report content.</param>
        /// <param name="containerName">The container.</param>
        /// <param name="storageConnectionString">The connectionString.</param>
        /// <returns></returns>
        Task SaveReport(string fileName, string fileContent, string containerName, string storageConnectionString);


        /// <summary>
        /// move file content.
        /// </summary>
        /// <param name="containerName">The container.</param>
        /// <param name="fileName">The fileName.</param>
        /// <param name="storageConnectionString">The connectionString.</param>
        /// <param name="archiveContainer">The archiveContainer.</param>
        /// <returns></returns>
        Task<bool> MoveFile(string containerName, string fileName, string storageConnectionString, string archiveContainer);
    }
}
