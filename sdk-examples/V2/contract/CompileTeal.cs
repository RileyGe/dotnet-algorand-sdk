using System;
using System.IO;
using System.Threading.Tasks;
using Algorand.V2;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;

namespace sdk_examples.V2.contract
{
    public class CompileTeal
    {
        public async Task Main(string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args[1];
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            
            // read file - int 1
            byte[] data = File.ReadAllBytes("V2\\contract\\sample.teal");
            CompileResponse response;
            using (var datams = new MemoryStream())
            {
                 response = await algodApiInstance.CompileAsync(datams);
            }

            Console.WriteLine("response: " + response);
            Console.WriteLine("Hash: " + response.Hash);
            Console.WriteLine("Result: " + response.Result);
            Console.ReadKey();

            //result
            //Hash: 6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY
            //Result: ASABASI=
        }
    }
}
