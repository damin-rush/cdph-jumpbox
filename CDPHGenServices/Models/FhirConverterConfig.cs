using System;
using System.Collections.Generic;
using System.Text;

namespace CDPHGenServices.Models
{
    public class FhirConverterConfig
    {
        public string Url { get; set; }
        public string EndPoint { get; set; }
        public string templateName { get; set; }
        public string AuthKey { get; set; }
        public string AuthValue { get; set; }

        public string GetUri()
        {
            return EndPoint + templateName;
        }
    }
}
