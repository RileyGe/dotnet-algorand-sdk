using System;
using System.IO;
using Algorand.V2;

namespace sdk_examples.V2.contract
{
    public class CompileTeal
    {
        public static void Main(string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args[1];            

            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            // read file - int 1
            byte[] data = File.ReadAllBytes("V2\\contract\\sample.teal");
            var response = algodApiInstance.TealCompile(data);

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
