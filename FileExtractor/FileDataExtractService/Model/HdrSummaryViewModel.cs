namespace FileDataExtractService.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HdrSummaryViewModel : BaseModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is RecordType.
        /// </summary>
        [JsonProperty(PropertyName = "RecordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is Statute.
        /// </summary>
        [JsonProperty(PropertyName = "Statute")]
        public string Statute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is RecordCountOnHDR.
        /// </summary>
        [JsonProperty(PropertyName = "RecordCountOnHDR")]
        public int RecordCountOnHDR { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the is PaymentAmount.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentAmount")]
        public double PaymentAmount { get; set; }
    }
}
