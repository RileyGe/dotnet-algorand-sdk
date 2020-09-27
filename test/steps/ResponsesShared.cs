using Algorand.V2;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TechTalk.SpecFlow;

namespace test.steps
{
    [Binding]
    public class ResponsesShared
    {
        public FileStream bodyFile;
        public Response response;

        public AlgodApi algod = new AlgodApi("localhost:123", "");
        public IndexerApi indexer = new IndexerApi("localhost:123", "");

        public ResponsesShared()
        {
            try
            {
                ClientMocker.infect(algod);
                ClientMocker.infect(indexer);
            }
            catch (IllegalAccessException e)
            {
                Assertions.fail("Failed to infect the client.");
            }
        }


        [Given("mock http responses in {string} loaded from {string}")]
        public void mockResponse(string file, string dir)
        {
            mockResponseWithStatus(file, dir, 200);
        }

        [Given("mock http responses in {string} loaded from {string} with status {int}.")]
        public void mockResponseWithStatus(string file, string dir, int status)
        {
            //this.bodyFile = new File("src/test/resources/" + dir + "/" + file);
            Assert.DoesNotThrow(() => { bodyFile = File.OpenRead("src/test/resources/" + dir + "/" + file); });

            //Assert.DoesNotThrow(this.bodyFile).exists();

            // Content type based on file extension.
            string contentType;
            switch (Path.GetExtension(file))
            {
                case "base64":
                    contentType = "application/msgpack";
                    break;
                case "json":
                default:
                    contentType = "application/json";
                    break;
            }
            ClientMocker.oneResponse(status, contentType, bodyFile);
        }

        [When("we make any {string} call to {string}.")]
        public void we_make_any_call_to(string client, string endpoint)
        {
            switch (client)
            {
                case "indexer":
                    {
                        switch (endpoint)
                        {
                            case "lookupAccountByID":
                                response = indexer.lookupAccountByID(new Address()).execute();
                                break;
                            case "lookupApplicationByID":
                                response = indexer.lookupApplicationByID(10L).execute();
                                break;
                            case "searchForApplications":
                                response = indexer.searchForApplications().execute();
                                break;
                            case "lookupAssetBalances":
                                response = indexer.lookupAssetBalances(10L).execute();
                                break;
                            case "lookupAssetByID":
                                response = indexer.lookupAssetByID(10L).execute();
                                break;
                            case "searchForAssets":
                                response = indexer.searchForAssets().execute();
                                break;
                            case "searchForAccounts":
                                response = indexer.searchForAccounts().execute();
                                break;
                            case "lookupAccountTransactions":
                                response = indexer.lookupAccountTransactions(new Address()).execute();
                                break;
                            case "lookupAssetTransactions":
                                response = indexer.lookupAssetTransactions(10L).execute();
                                break;
                            case "searchForTransactions":
                            case "any": // error case, everything uses the same error message
                                response = indexer.searchForTransactions().execute();
                                break;
                            default:
                                Assertions.fail("Unsupported indexer endpoint: " + endpoint);
                        }
                        break;
                    }
                case "algod":
                    {
                        switch (endpoint)
                        {
                            case "GetStatus":
                                response = algod.GetStatus().execute();
                                break;
                            case "WaitForBlock":
                                response = algod.WaitForBlock(10L).execute();
                                break;
                            case "TealCompile":
                                response = algod.TealCompile().source("awesome teal program".getBytes()).execute();
                                break;
                            case "RawTransaction":
                                response = algod.RawTransaction().rawtxn("transaction".getBytes()).execute();
                                break;
                            case "GetSupply":
                                response = algod.GetSupply().execute();
                                break;
                            case "GetApplicationByID":
                                response = algod.GetApplicationByID(10L).execute();
                                break;
                            case "GetAssetByID":
                                response = algod.GetAssetByID(10L).execute();
                                break;
                            case "TransactionParams":
                            case "any": // error case, everything uses the same error message
                                response = algod.TransactionParams().execute();
                                break;
                            case "PendingTransactionInformation":
                                response = algod.PendingTransactionInformation("transaction").execute();
                                break;
                            case "GetPendingTransactions":
                                response = algod.GetPendingTransactions().execute();
                                break;
                            case "GetPendingTransactionsByAddress":
                                response = algod.GetPendingTransactionsByAddress(new Address()).execute();
                                break;
                            default:
                                Assertions.fail("Unsupported algod endpoint: " + endpoint);
                        }
                        break;
                    }
            }
        }

        [Then("the parsed response should equal the mock response.")]
        public void the_parsed_response_should_equal()
        {
            verifyResponse(response, bodyFile);
        }
    }
}
