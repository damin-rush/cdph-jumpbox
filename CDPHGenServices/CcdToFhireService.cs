using CDPHGenServices.Models;
using RestSharp;
using Microsoft.Extensions.Logging;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace CDPHGenServices
{
    public class CcdToFhireService : ICcdToFhireService
    {
        private FhirConverterConfig _config;
        private ILoggerFactory _loggerFactory;
        private readonly ILogger<CcdToFhireService> _logger;
        

        public CcdToFhireService(FhirConverterConfig FhirConverterConfig)
        {
            _config = FhirConverterConfig;
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    //.AddConfiguration(_config.GetSection("Logging"))
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("CcdToFhireServices", LogLevel.Information);
                    //.AddConsole();
                    //.AddEventLog();
            });

            _logger = _loggerFactory.CreateLogger<CcdToFhireService>();
        }

        public string ConvertCcdToFhir(Stream ccd, bool resetStreamPosition = true)
        {
            try
            {
                _logger.LogInformation("In ConvertCcdToFhir with stream length at {0}", ccd.Length);
                if (resetStreamPosition == true)
                {
                    ccd.Position = 0;
                }

                using (var sr = new StreamReader(ccd))
                {
                    return ConvertCcdToFhir(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Converts CCd document to FHIR
        /// </summary>
        /// <param name="ccd"></param>
        /// <returns></returns>
        public string ConvertCcdToFhir(string ccd)
        {
            try
            {
                var client = new RestClient(_config.Url);
                _logger.LogInformation("Converter URL is {0}", _config.Url);
                //client.Authenticator = new HttpBasicAuthenticator(_config.AuthKey, _config.AuthValue);

                _logger.LogInformation("Converter URI is {0}", _config.GetUri());
                var request = new RestRequest(_config.GetUri(), Method.POST);

                _logger.LogInformation("Converter AuthKey is {0} and AuthValue is {1}", _config.AuthKey, _config.AuthValue);
                _logger.LogInformation("CCD value is {0}", ccd);
                request.AddParameter(_config.AuthKey, _config.AuthValue, ParameterType.HttpHeader);
                request.AddParameter("text/plain", ccd, ParameterType.RequestBody);

                var response = client.Post(request);

                if(response.IsSuccessful == false)
                {
                    throw new Exception("Could not convert to FHIR");
                }

                return response.Content;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
