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
        /// <param name="fileContent">The report content.</param>
        /// <param name="containerName">The container.</param>
        /// <param name="storageConnectionString">The connectionString.</param>
        /// <returns></returns>
        Task SaveReport(string fileContent, string containerName, string storageConnectionString);
    }
}
