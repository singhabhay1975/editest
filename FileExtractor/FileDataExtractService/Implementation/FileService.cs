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
            //var secondFileTypeList = fileList.FindAll(x => x.Contains(".rst") || x.Contains(".ct") || x.Contains(".eht"));
            var secondFileTypeList = fileList.FindAll(x => x.Contains(".eht"));
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
                        // TEMP 
                        if (line.Substring(0, 3) != "HDR") // NON EHT
                        {
                            break; //SKIP FOR NOW
                        }

                        // TEMP
                        if (i == 0)
                        {

                            // Payment amount has no decimals. It has implied decimal places for cents.  should be converted to double and divided by 100
                            data.Add(new HdrSummaryViewModel { FileName = file, RecordType = line.Substring(0, 3), PaymentAmount = double.Parse(line.Substring(17, 15))/100, 
                                RecordCountOnHDR = int.Parse(line.Substring(32, 6)),
                                Statute = line.Substring(38, 3),
                                TotalLine = fileLines.Count -1 });
                            break;
                        }
                    }
                }
            }

            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("Record Type ");
            fileReport.Append("Payment Amount ");
            fileReport.Append("RecordCountOnHDR ");
            fileReport.Append("Statute ");
            fileReport.Append("TotalNumberOfDetailRecords  ");
            fileReport.Append(Environment.NewLine);
            fileReport.Append("-----------------------------------------------");
            foreach (var d in data)
            {
                fileReport.Append(Environment.NewLine);
                fileReport.Append(d.FileName);
                fileReport.Append(" ");
                fileReport.Append(d.RecordType);
                fileReport.Append(" ");
                fileReport.Append(d.PaymentAmount);
                fileReport.Append(" ");
                fileReport.Append(d.RecordCountOnHDR);
                fileReport.Append(" ");
                fileReport.Append(d.Statute);
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
                    string isa_05 = "not found"; // ISA record variable. Bank.
                    for (var i = 0; i < fileLines.Count; i++)
                    {
                        var line = fileLines[i];
                        string str = line.Substring(0, 2);
                        var columnList = new ArrayList();
                        

                        if (line.Substring(0, 3).ToUpper().Trim() == "ISA") // ISA RECORD
                        {
                            //EXTRACT BANK ISA_05
                             
                            isa_05 = fileLines[i].Split(delimeter)[5]; 

                        }


                        if (str.ToUpper().Trim() == "ST") 
                        {
                           var dataRecord = new List<ArrayList>();
                            var entCount = 0;
                            double bpr_02 = 0;
                            string n1_02 = "not found"; // agency
                            string rmr_02 = "not found"; // statute
                            var bpr_16 = 0;
                            
                            var j = 0;
                            for(j=i; j< fileLines.Count; j++)
                            {
                                str = fileLines[j].Substring(0, 2);
                                // ST - SE IS A BLOCK OF TRANSACTIONS. They create one output file 2. 
                                // COUNT ALL ENT'S IN THE ST - SE BLOCK
                                if (str.ToUpper().Trim() != "ST")
                                {
                                    // COUNT AND STORE ENT'S
                                     
                                    if (fileLines[j].Substring(0, 3).ToUpper().Trim() == "ENT")
                                    {
                                        entCount++; // count ent's within ST/SE BLOCK
                                        addedST = true;
                                        columnList.Add(fileLines[j].Split(delimeter));
                                        dataRecord.Add(columnList);
                                    }
                                    // extract bpr_02 Payment amount
                                    // extract bpr_16 deposit date
                                    if (fileLines[j].Substring(0, 3).ToUpper().Trim() == "BPR")
                                    {
                                        string[] bpr = fileLines[j].Split(delimeter);
                                        for (int x = 0; x < bpr.Length; x++)
                                        {
                                            
                                            if (x == 2)
                                            {
                                                bpr_02 = double.Parse(bpr[2]);
                                                 
                                            }
                                            if (x == 16)
                                            {
                                                bpr_16 = int.Parse(bpr[16]);
                                                
                                            }



                                        }
                                                                             
                                    }

                                    // extract rmr_02 statute
                          
                                    if (fileLines[j].Substring(0, 3).ToUpper().Trim() == "RMR")
                                    {
                                        string[] rmr = fileLines[j].Split(delimeter);
                                        for (int x = 0; x < rmr.Length; x++)
                                        {

                                            if (x == 2)
                                            {
                                                rmr_02 = rmr[2];

                                            }                                      

                                        }

                                    }

                                    // extract N1_02 Agency

                                    if (fileLines[j].Substring(0, 2).ToUpper().Trim() == "N1")
                                    {
                                        string[] n1 = fileLines[j].Split(delimeter);
                                        for (int x = 0; x < n1.Length; x++)
                                        {

                                            if (x == 1 & n1[1] == "AG")
                                            {
                                                n1_02 = n1[2];
                                                //break;

                                            }                                    

                                        }

                                    }
                                    
                                }
                                else if(addedST)
                                {
                                    addedST = false;
                                    break;
                                }
                            }

                            i = j-1;

                            sftpFileData.Add(new SftpFileViewModel { FileName = file, DataRecord = dataRecord, Stline = line, entCount = entCount,
                                rmr_02 = rmr_02, bpr_02 = bpr_02, bpr_16 = bpr_16, isa_05 = isa_05, n1_02 = n1_02 });
                        }
                    }
                }
            }

            StringBuilder fileReport = new StringBuilder();
            fileReport.Append("FileName ");
            fileReport.Append("Record ");
            fileReport.Append("TotalRecord ");
            fileReport.Append("EntTransactionCOUNT ");
            fileReport.Append("RMR_02 Statute  ");
            fileReport.Append("BPR_02 Payment Amount  ");
            fileReport.Append("BPR_16 Deposit Date  ");
            fileReport.Append("ISA_05 Bank  ");
            fileReport.Append("N1_02 Agency  ");
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
                fileReport.Append(d.entCount);
                fileReport.Append(" ");
                fileReport.Append(d.rmr_02);
                fileReport.Append(" ");
                fileReport.Append(d.bpr_02);
                fileReport.Append(" ");
                fileReport.Append(d.bpr_16);
                fileReport.Append(" ");
                fileReport.Append(d.isa_05);
                fileReport.Append(" ");
                fileReport.Append(d.n1_02);
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
           // await this.blobWrapper.SaveReport("report", fileReport.ToString(), this.outputContainer, this.blobConnectionString).ConfigureAwait(false);

            return sftpFileData;
        }
    }
}
