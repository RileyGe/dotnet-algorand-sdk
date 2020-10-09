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
        /// <param name="timeout">time out.</param>
        public AlgodApi(string host, string token, int timeout = -1)
        {
            Configuration config = new Configuration
            {
                BasePath = host                
            };
            if (host.Contains("algorand.api.purestake.io"))
                //config.ApiKey.Add(new KeyValuePair<string, string>("X-API-Key", apiToken)); //purestake
                config.AddDefaultHeader("X-API-Key", token);
            
            config.ApiKey.Add(new KeyValuePair<string, string>("X-Algo-API-Token", token));

            if (timeout > 0)
            {
                config.Timeout = timeout;
            }
            this.Configuration = config;
            //super(host, port, token, "X-Algo-API-Token");
        }        
    }
}
