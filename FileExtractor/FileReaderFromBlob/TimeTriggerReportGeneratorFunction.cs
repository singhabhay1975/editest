namespace FileReaderFromBlob
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using FileDataExtractService.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Logging;

    public class TimeTriggerReportGeneratorFunction
    {
        /// <summary>
        /// The file services. Repo change...
        /// </summary>
        private readonly IFileServices fileService;

        /// <summary>
        /// Construct an object of the type file data reader function.
        /// </summary>
        /// <param name="fileService">The fileservice.</param>
        public TimeTriggerReportGeneratorFunction(IFileServices fileService)
        {
            this.fileService = fileService;
        }

        [FunctionName("TimeTriggerReportGeneratorFunction")]
        public async Task Run([TimerTrigger("%timer-frequecy%")]TimerInfo myTimer)
        {
            await this.fileService.GetBlobFileInfo().ConfigureAwait(false);
        }
    }
}
