using Algorand.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand.Kmd.Api
{
    public class KmdApi : DefaultApi
    {
        public KmdApi() : base() { }
        public KmdApi(string basePath) : base(basePath) { }
        public KmdApi(Configuration configuration = null): base(configuration) { }
    }
}
