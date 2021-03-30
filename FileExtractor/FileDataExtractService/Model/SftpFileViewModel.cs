namespace FileDataExtractService.Model
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;

    public class SftpFileViewModel : BaseModel
    {
        public SftpFileViewModel()
        {
            this.DataRecord = new List<ArrayList>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is fileName.
        /// </summary>
        [JsonProperty(PropertyName = "dataRecord")]
        public List<ArrayList> DataRecord { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is stline.
        /// </summary>
        [JsonProperty(PropertyName = "stline")]
        public string Stline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is stline.
        /// </summary>
        [JsonProperty(PropertyName = "entCount")]
        public int entCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is bpr_02.
        /// </summary>
        [JsonProperty(PropertyName = "bpr_02")]
        public double bpr_02 { get; set; }

        [JsonProperty(PropertyName = "bpr_16")]
        public int bpr_16 { get; set; }

        [JsonProperty(PropertyName = "isa_05")]
        public string isa_05 { get; set; }

        [JsonProperty(PropertyName = "n1_02")]
        public string n1_02 { get; set; }

        [JsonProperty(PropertyName = "rmr_02")]
        public string rmr_02 { get; set; }

    }
}
