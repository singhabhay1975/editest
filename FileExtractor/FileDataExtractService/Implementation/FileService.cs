namespace FileDataExtractService.Implementation
{
    using DocumentFormat.OpenXml.Office.CustomUI;
    using FileDataExtractService.Interface;
    using FileDataExtractService.Model;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
        /// The archiece blob container name.
        /// </summary>
        private readonly string archiveContainer;

        /// <summary>
        /// Construct a public objectof FileService.
        /// </summary>
        /// <param name="blobWrapper">The blobwrapper.</param>
        public FileService(IBlobWrapper blobWrapper, IConfiguration configuration)
        {
            this.blobWrapper = blobWrapper;
            this.configuration = configuration;
            this.blobConnectionString = this.configuration["AzureWebJobsStorage"];
            this.rawFileContainter = "edireconinput";
            this.outputContainer = "edireconoutput";
            this.archiveContainer = "edireconarchive";
        }

        public async Task<string> GetBlobFileInfo()
        {
            var fileList = await this.blobWrapper.GetBlobFileListAsync(this.blobConnectionString, this.rawFileContainter).ConfigureAwait(false);
            var firstFileTypeList = fileList.FindAll(x => x.Contains("SFTP"));
            var secondFileTypeList = fileList.FindAll(x => x.Contains(".rst") || x.Contains(".ct") || x.Contains(".eht"));
            var thirdFileTypeList = fileList.FindAll(x => x.Contains("EDI"));

            await this.GetSftpFileData(firstFileTypeList).ConfigureAwait(false);
            await this.GetHdrDtlData(secondFileTypeList).ConfigureAwait(false);
            await this.GetEdiData(thirdFileTypeList).ConfigureAwait(false);

            foreach(var file in fileList)
            {
                await this.blobWrapper.MoveFile(this.rawFileContainter, file, this.blobConnectionString, this.archiveContainer).ConfigureAwait(false);
            }

            return "";
        }


        public async Task<List<EdiDataFileViewModel>> GetEdiData(List<string> fileList)
        {
            var data = new List<EdiDataFileViewModel>();
            if (fileList != null)
            {
                foreach (var file in fileList)
                {
                    var fileLines = await this.blobWrapper.GetFile(this.rawFileContainter, file, this.blobConnectionString).ConfigureAwait(false);
                    data.Add(new EdiDataFileViewModel { FileName = file, Data = fileLines, TotalLine = fileLines.Count });
                }
            }

            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("Total number of Records");
            fileReport.Append(Environment.NewLine);
            fileReport.Append("-----------------------------------------------");
            fileReport.Append(Environment.NewLine);
            foreach (var d in data)
            {
                fileReport.Append(d.FileName);
                fileReport.Append(" ");
                fileReport.Append(d.TotalLine);
                fileReport.Append(" ");
                fileReport.Append(Environment.NewLine);
                fileReport.Append("Contents");
                foreach (var l in d.Data)
                {
                    fileReport.Append(l);
                    fileReport.Append(Environment.NewLine);
                }
                fileReport.Append(Environment.NewLine);
                fileReport.Append("-----------------------------------------------");
            }

            await this.blobWrapper.SaveReport("edireconinput3", fileReport.ToString(), this.outputContainer, this.blobConnectionString).ConfigureAwait(false);

            return data;
        }

        public async Task<List<HdrSummaryViewModel>> GetHdrDtlData(List<string> fileList)
        {
            var data = new List<HdrSummaryViewModel>();
            if (fileList != null)
            {
                foreach (var file in fileList)
                {
                    var fileLines = await this.blobWrapper.GetFile(this.rawFileContainter, file, this.blobConnectionString).ConfigureAwait(false);
                    for (var i = 0; i < fileLines.Count; i++)
                    {
                        var line = fileLines[i];
                        if (i == 0)
                        {
                            data.Add(new HdrSummaryViewModel { FileName = file, Type = line.Substring(0, 3), Date = line.Substring(10, 17), TransmissionNumber= int.Parse(line.Substring(4, 9)), TotalLine = fileLines.Count -1 });
                            break;
                        }
                    }
                }
            }

            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("Type ");
            fileReport.Append("Date ");
            fileReport.Append("TransmissionNumber ");
            fileReport.Append("Totalnumberofdetailrecords  ");
            fileReport.Append(Environment.NewLine);
            fileReport.Append("-----------------------------------------------");
            foreach (var d in data)
            {
                fileReport.Append(Environment.NewLine);
                fileReport.Append(d.FileName);
                fileReport.Append(" ");
                fileReport.Append(d.Type);
                fileReport.Append(" ");
                fileReport.Append(d.Date);
                fileReport.Append(" ");
                fileReport.Append(d.TransmissionNumber);
                fileReport.Append(" ");
                fileReport.Append(d.TotalLine);
                fileReport.Append(" ");
                fileReport.Append(Environment.NewLine);
               // fileReport.Append("Contents");
                //foreach (var l in d.DataRecord)
                //{
                //    fileReport.Append(l);
                //    fileReport.Append(Environment.NewLine);
                //}
                fileReport.Append(Environment.NewLine);
                fileReport.Append("-----------------------------------------------");
            }

            await this.blobWrapper.SaveReport("edireconinput2", fileReport.ToString(), this.outputContainer, this.blobConnectionString).ConfigureAwait(false);

            return data;
        }

        public async Task<List<SftpFileViewModel>> GetSftpFileData(List<string> fileList)
        {
            var sftpFileData = new List<SftpFileViewModel>(); 
            char delimeter = ',';

            if (fileList != null)
            {
                foreach (var file in fileList)
                {
                    var fileLines = await this.blobWrapper.GetFile(this.rawFileContainter, file, this.blobConnectionString).ConfigureAwait(false);
                    for(var i =0; i<fileLines.Count; i++)
                    {
                        var line = fileLines[i];
                        if (i == 0)
                        {
                            for (int j = 0; j < line.Length; j++)
                            {
                                if (j == 3)
                                {
                                    delimeter = line[j];
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    var addedST = false;
                    for (var i = 0; i < fileLines.Count; i++)
                    {
                        var line = fileLines[i];
                        string str = line.Substring(0, 2);
                        var columnList = new ArrayList();
                        if (str.ToUpper().Trim() == "ST")
                        {
                           var dataRecord = new List<ArrayList>();
                            sftpFileData.Add(new SftpFileViewModel { FileName = file, DataRecord = dataRecord, Stline = line });
                            var j = 0;
                            for(j=i; j< fileLines.Count; j++)
                            {
                                str = fileLines[j].Substring(0, 2);
                                if (str.ToUpper().Trim() != "ST")
                                {
                                    addedST = true;
                                    columnList.Add(fileLines[j].Split(delimeter));
                                    dataRecord.Add(columnList);
                                }
                                else if(addedST)
                                {
                                    addedST = false;
                                    break;
                                }
                            }

                            i = j-1;
                        }
                    }
                }
            }

            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("ST ");
            fileReport.Append("TotalRecord ");
            fileReport.Append(Environment.NewLine);
            fileReport.Append("-----------------------------------------------");
            foreach (var d in sftpFileData)
            {
                fileReport.Append(Environment.NewLine);
                fileReport.Append(d.FileName);
                fileReport.Append(" ");
                fileReport.Append(d.Stline);
                fileReport.Append(" ");
                fileReport.Append(d.DataRecord.Count);
                fileReport.Append(" ");
                fileReport.Append(Environment.NewLine);
               // fileReport.Append("Contents");
                //foreach (var l in d.DataRecord)
                //{
                //    fileReport.Append(l);
                //    fileReport.Append(Environment.NewLine);
                //}
                fileReport.Append(Environment.NewLine);
             //   fileReport.Append("-----------------------------------------------");
            }

            await this.blobWrapper.SaveReport("edireconinput1", fileReport.ToString(), this.outputContainer, this.blobConnectionString).ConfigureAwait(false);

            return sftpFileData;
        }
    }
}
