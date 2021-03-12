namespace FileDataExtractService.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HdrSummaryViewModel : BaseModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is type.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is transmissionNumber.
        /// </summary>
        [JsonProperty(PropertyName = "transmissionNumber")]
        public int TransmissionNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is date.
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
    }
}
