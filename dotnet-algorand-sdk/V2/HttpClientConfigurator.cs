using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Text;

namespace Algorand.V2
{
    public  class HttpClientConfigurator
    {
        public static HttpClient ConfigureHttpClient(string host, string token, string tokenHeader = "", int timeout = -1)
        {
            var _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(tokenHeader))
            {
                if (host.Contains("algorand.api.purestake.io") || host.Contains("bsngate.com/api"))
                {
                    tokenHeader = "X-API-Key";
                }
                else
                {
                    tokenHeader = "X-Algo-API-Token";
                }
            }
            
            if (tokenHeader != null && tokenHeader.Length > 0)
                _httpClient.DefaultRequestHeaders.Add(tokenHeader, token);

            _httpClient.Timeout = timeout > 0 ? (TimeSpan.FromMilliseconds((double)timeout)) : Timeout.InfiniteTimeSpan;

            return _httpClient;

        }
    }
}
