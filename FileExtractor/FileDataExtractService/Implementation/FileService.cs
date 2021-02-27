namespace FileDataExtractService.Implementation
{
    using FileDataExtractService.Interface;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class FileService : IFileServices
    {
        /// <summary>
        /// The blob wrapper.
        /// </summary>
        private readonly IBlobWrapper blobWrapper;

        /// <summary>
        /// the configuration.
        /// </summary>
        private IConfiguration configuration { get; set; }

        /// <summary>
        /// The blob connection string.
        /// </summary>
        private readonly string blobConnectionString;

        /// <summary>
        /// The blob container name.
        /// </summary>
        private readonly string rawFileContainter;

        /// <summary>
        /// The blob container name.
        /// </summary>
        private readonly string outputContainer;

        /// <summary>
        /// Construct a public objectof FileService.
        /// </summary>
        /// <param name="blobWrapper">The blobwrapper.</param>
        public FileService(IBlobWrapper blobWrapper, IConfiguration configuration)
        {
            this.blobWrapper = blobWrapper;
            this.configuration = configuration;
            this.blobConnectionString = this.configuration["AzureWebJobsStorage"];
            this.rawFileContainter = "filecontainer";
            this.outputContainer = "reportoutput";
        }

        public async Task<string> GetBlobFileInfo()
        {
            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("Total Records");
            fileReport.Append(Environment.NewLine);
            fileReport.Append("-----------------------------------------------");
            fileReport.Append(Environment.NewLine);
            var fileList = await this.blobWrapper.GetBlobFileListAsync(this.blobConnectionString, this.rawFileContainter).ConfigureAwait(false);
            if (fileList != null)
            {
                foreach (var file in fileList)
                {
                    var fileLines = await this.blobWrapper.GetFile(this.rawFileContainter, file, this.blobConnectionString).ConfigureAwait(false);
                    fileReport.Append(file + " ");
                    fileReport.Append(fileLines.Count);
                    fileReport.Append(Environment.NewLine);
                    //foreach(var line in fileLines)
                    // {
                    //     string[] values = line.Split(',');
                    //     float[] numbers = new float[values.Length - 1];
                    //     for (int i = 1; i < values.Length - 1; i++)
                    //         numbers[i - 1] = float.Parse(values[i]);
                    // }
                }
            }

            fileReport.Append("-----------------------------------------------");

            await this.blobWrapper.SaveReport(fileReport.ToString(), this.outputContainer, this.blobConnectionString).ConfigureAwait(false);
            return fileReport.ToString();
        }
    }
}
