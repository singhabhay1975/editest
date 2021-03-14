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

    }
}
