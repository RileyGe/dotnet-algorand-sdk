using Algorand.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand.Algod.Api
{
    /// <summary>
    /// Rename DefautlApi
    /// </summary>
    public class AlgodApi : DefaultApi
    {
        //public AlgodApi() : base() { }
        //public AlgodApi(string basePath) : base(basePath) { }
        //public AlgodApi(Configuration configuration = null) : base(configuration) { }
        public AlgodApi(string bathPath, string apiToken, int timeout = -1)
        {
            Configuration config = new Configuration
            {
                BasePath = bathPath
            };
            if (bathPath.Contains("algorand.api.purestake.io"))
                //config.ApiKey.Add(new KeyValuePair<string, string>("X-API-Key", apiToken)); //purestake
                config.AddDefaultHeader("X-API-Key", apiToken);
            else
                config.ApiKey.Add(new KeyValuePair<string, string>("X-Algo-API-Token", apiToken));

            
            if (timeout > 0)
            {
                config.Timeout = timeout;
            }
            this.Configuration = config;
        }
        
    }
}
