namespace FileReaderFromBlob
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using FileDataExtractService.Interface;
    using System;
    using System.Text.RegularExpressions;

    public class FileDataReaderFunction
    {
        /// <summary>
        /// The file services.
        /// </summary>
        private readonly IFileServices fileService;

        /// <summary>
        /// Construct an object of the type file data reader function.
        /// </summary>
        /// <param name="fileService">The fileservice.</param>
        public FileDataReaderFunction(IFileServices fileService)
        {
            this.fileService = fileService;
        }

        [FunctionName("FileDataReaderFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            var fileReport = await this.fileService.GetBlobFileInfo().ConfigureAwait(false);
            if (fileReport?.Length > 0)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(fileReport);
                        writer.Flush();
                        stream.Position = 0;
                        return new FileContentResult(stream.ToArray(), "application/octet-stream")
                        {
                            FileDownloadName = "Report-Gernated-"+ Regex.Replace(DateTime.UtcNow.ToString(), "[^a-zA-Z0-9% ._]", string.Empty)+".txt",
                        };
                    }
                }
            }
            else
            {
                return new OkObjectResult("No data found");
            }
        }
    }
}
