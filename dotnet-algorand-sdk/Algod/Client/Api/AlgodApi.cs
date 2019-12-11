using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand.Algod.Client.Api
{
    /// <summary>
    /// Rename DefautlApi
    /// </summary>
    public class AlgodApi : DefaultApi
    {
        //public AlgodApi() : base() { }
        //public AlgodApi(string basePath) : base(basePath) { }
        //public AlgodApi(Configuration configuration = null) : base(configuration) { }
        public AlgodApi(string bathPath, string apiToken)
        {
            Configuration config = new Configuration
            {
                BasePath = bathPath
            };
            config.ApiKey.Add(new KeyValuePair<string, string>("X-Algo-API-Token", apiToken));
            this.Configuration = config;
        }
    }
}
