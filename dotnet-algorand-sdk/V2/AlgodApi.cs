using Algorand.V2.Algod;
using Algorand.Client;
using System.Collections.Generic;

namespace Algorand.V2
{
    /// <summary>
    /// an AlgodClient for communicating with the REST API.
    /// </summary>
    public class AlgodApi: DefaultApi
    {
        /// <summary>
        /// Construct an AlgodClient for communicating with the REST API.
        /// </summary>
        /// <param name="host">using a URI format.If the scheme is not supplied the client will use HTTP.</param>
        /// <param name="token">authentication token.</param>
        /// <param name="tokenHeader">If you are using the API service, set this param with the header name.
        /// Purestake and psn.algorand.org API service can set the name automatically.</param>
        /// <param name="timeout">time out.</param>
        public AlgodApi(string host, string token, string tokenHeader = "", int timeout = -1)
        {
            Configuration config = new Configuration {
                BasePath = host                
            };
            //purestake or bsn.algorand.org
            if (host.Contains("algorand.api.purestake.io") || host.Contains("bsngate.com/api"))                
                tokenHeader = "X-API-Key";
            
            if(tokenHeader != null && tokenHeader.Length > 0)
                config.AddDefaultHeader(tokenHeader, token);

            config.ApiKey.Add(new KeyValuePair<string, string>("X-Algo-API-Token", token));

            if (timeout > 0) config.Timeout = timeout;            
            this.Configuration = config;
        }        
    }
}
