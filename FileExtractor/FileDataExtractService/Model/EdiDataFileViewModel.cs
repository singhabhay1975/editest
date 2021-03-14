namespace FileDataExtractService.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class EdiDataFileViewModel : BaseModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is data.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public List<string> Data { get; set; }
    }
}
