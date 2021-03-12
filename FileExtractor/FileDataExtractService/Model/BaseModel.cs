namespace FileDataExtractService.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class BaseModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is fileName.
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is totalLIne.
        /// </summary>
        [JsonProperty(PropertyName = "totalLine")]
        public int TotalLine { get; set; }
    }
}
