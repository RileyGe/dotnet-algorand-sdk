using Algorand;

using Algorand.V2;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Account = Algorand.Account;
using System.Text;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;
using System.Threading.Tasks;

namespace sdk_examples.V2.contract
{
    class StatefulContract
    {
        public  async Task Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
            //第一个账号用于给智能合约签名，并把签名发布出去
            string adminMnemonic = "place blouse sad pigeon wing warrior wild script"
                    + " problem team blouse camp soldier breeze twist mother"
                    + " vanish public glass code arrow execute convince ability there";
            Account admin = new Account(adminMnemonic);
            var creatorMnemonic = "benefit once mutual legal marble hurdle dress toe fuel country prepare canvas barrel divide major square name captain calm flock crumble receive economy abandon power";
            var userMnemonic = "pledge become mouse fantasy matrix bunker ask tissue prepare vocal unit patient cliff index train network intact company across stage faculty master mom abstract above";
            // create two account to create and user the stateful contract
            var creator = new Account(creatorMnemonic);
            var user = new Account(userMnemonic);
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi client = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };

            //var transParams = client.TransactionParams();
            //var tx = Utils.GetPaymentTransaction(admin.Address, creator.Address, 
            //    Utils.AlgosToMicroalgos(1), null, transParams);
            //var resp = Utils.SubmitTransaction(client, admin.SignTransaction(tx));
            //var waitResp = Utils.WaitTransactionToComplete(client, resp.TxId);
            //Console.WriteLine(string.Format("send 1 algo to account {0} at round {1}",
            //    creator.Address.ToString(), waitResp.ConfirmedRound));

            //tx = Utils.GetPaymentTransaction(admin.Address, user.Address,
            //    Utils.AlgosToMicroalgos(1), null, transParams);
            //resp = Utils.SubmitTransaction(client, admin.SignTransaction(tx));
            //waitResp = Utils.WaitTransactionToComplete(client, resp.TxId);
            //Console.WriteLine(string.Format("send 1 algo to account {0} at round {1}",
            //    user.Address.ToString(), waitResp.ConfirmedRound));

            // declare application state storage (immutable)
            ulong localInts = 1;
            ulong localBytes = 1;
            ulong globalInts = 1;
            ulong globalBytes = 0;

            // user declared approval program (initial)
            string approvalProgramSourceInitial = File.ReadAllText("stateful_approval_init.teal");

            // user declared approval program (refactored)
            string approvalProgramSourceRefactored = File.ReadAllText("stateful_approval_refact.teal");
            // creator 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4
            // user GG7UDCTXNHADKSJ22GG64BZNKXXLXMSYWVZDD2UGHBZ6RLVXWGRLMW52DU
            // declare clear state program source
            string clearProgramSource = File.ReadAllText("stateful_clear.teal");

            CompileResponse approvalProgram;
            CompileResponse clearProgram;
            CompileResponse approvalProgramRefactored;

            using (var datams = new MemoryStream(Encoding.UTF8.GetBytes(approvalProgramSourceInitial)))
            {
                approvalProgram = await client.CompileAsync(datams);
            }
            using (var datams = new MemoryStream(Encoding.UTF8.GetBytes(clearProgramSource)))
            {
                clearProgram = await client.CompileAsync(datams);
            }
            using (var datams = new MemoryStream(Encoding.UTF8.GetBytes(approvalProgramSourceRefactored)))
            {
                approvalProgramRefactored = await client.CompileAsync(datams);
            }
         
           

            try
            {
                // create new application
                var appid = await CreateApp(client, creator, new TEALProgram(approvalProgram.Result),
                    new TEALProgram(clearProgram.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                await OptIn(client, user, appid);
                // call application without arguments
                await CallApp(client, user, appid, null);
                // read local state of application from user account
                await ReadLocalState(client, user, appid);

                // read global state of application
                await ReadGlobalState(client, creator, appid);

                // update application
                await UpdateApp(client, creator, appid,
                    new TEALProgram(approvalProgramRefactored.Result),
                    new TEALProgram(clearProgram.Result));
                // call application with arguments
                var date = DateTime.Now;
                Console.WriteLine(date.ToString("yyyy-MM-dd 'at' HH:mm:ss"));
                List<byte[]> appArgs = new List<byte[]>
                {
                    Encoding.UTF8.GetBytes(date.ToString("yyyy-MM-dd 'at' HH:mm:ss"))
                };
                await CallApp(client, user, appid, appArgs);

                // read local state of application from user account
                await ReadLocalState(client, user, appid);

                // close-out from application
                await CloseOutApp(client, user, (ulong)appid);

                // opt-in again to application
                await OptIn(client, user, appid);

                // call application with arguments
                await CallApp(client, user, appid, appArgs);

                // read local state of application from user account
                await ReadLocalState(client, user, appid);

                // delete application
                await DeleteApp(client, creator, appid);

                // clear application from user account
                await ClearApp(client, user, appid);

                Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#sendTransaction: " + e.Message);
            }
        }
        public static async Task CloseOutApp(DefaultApi client, Account sender, ulong appId)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCloseTransaction(sender.Address, (ulong?)appId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
                Console.WriteLine("Application ID is: " + appId);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static async Task UpdateApp(DefaultApi client, Account creator, ulong? appid, TEALProgram approvalProgram, TEALProgram clearProgram)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationUpdateTransaction(creator.Address, (ulong?)appid, approvalProgram, clearProgram, transParams);
                var signedTx = creator.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
                Console.WriteLine("Application ID is: " + appid);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static async Task<ulong?> CreateApp(DefaultApi client, Account creator, TEALProgram approvalProgram,
            TEALProgram clearProgram, ulong globalInts, ulong globalBytes, ulong localInts, ulong localBytes)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCreateTransaction(creator.Address, approvalProgram, clearProgram,
                    new Algorand.V2.Indexer.Model.StateSchema() { NumUint = globalInts, NumByteSlice = globalBytes }, new Algorand.V2.Indexer.Model.StateSchema() { NumUint = localInts, NumByteSlice = localBytes }, transParams);
                var signedTx = creator.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Application ID is: " + resp.ApplicationIndex.ToString());
                return resp.ApplicationIndex;
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
                return null;
            }
        }

        static async Task OptIn(DefaultApi client, Account sender, ulong? applicationId)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationOptinTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine(string.Format("Address {0} optin to Application({1})",
                    sender.Address.ToString(), (resp.Txn as JObject)["txn"]["apid"]));
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static async Task DeleteApp(DefaultApi client, Account sender, ulong? applicationId)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationDeleteTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Success deleted the application " + (resp.Txn as JObject)["txn"]["apid"]);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static async Task ClearApp(DefaultApi client, Account sender, ulong? applicationId)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationClearTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Success cleared the application " + (resp.Txn as JObject)["txn"]["apid"]);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static async Task CallApp(DefaultApi client, Account sender, ulong? applicationId, List<byte[]> args)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCallTransaction(sender.Address, (ulong?)applicationId, transParams, args);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed at round: " + resp.ConfirmedRound);
                Console.WriteLine(string.Format("Call Application({0}) success.", 
                    (resp.Txn as JObject)["txn"]["apid"]));
                //System.out.println("Called app-id: " + pTrx.txn.tx.applicationId);
                if (resp.GlobalStateDelta != null)
                {
                    var outStr = "    Global state: ";
                    foreach(var v in resp.GlobalStateDelta)
                    {
                        outStr += v.ToString();
                    }
                    Console.WriteLine(outStr);
                }
                if (resp.LocalStateDelta != null)
                {
                    var outStr = "    Local state: ";
                    foreach(var v in resp.LocalStateDelta)
                    {
                        outStr += v.ToString();
                    }
                    Console.WriteLine(outStr);
                }
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static public async Task ReadLocalState(DefaultApi client, Account account, ulong? appId)
        {
            var acctResponse = await client.AccountsAsync(account.Address.ToString(),null);
            var applicationLocalState = acctResponse.AppsLocalState;
            foreach (var state in applicationLocalState)
            {
                if (state.Id == appId)
                {
                    var outStr = "User's application local state: ";
                    foreach (var v in state.KeyValue)
                    {
                        outStr += v.ToString();
                    }
                    Console.WriteLine(outStr);
                }
            }
        }

        static public async Task ReadGlobalState(DefaultApi client, Account account, ulong? appId)
        {
            var acctResponse = await client.AccountsAsync(account.Address.ToString(),null);
            var createdApplications = acctResponse.CreatedApps;
            foreach( var app in createdApplications)
            {
                if (app.Id == appId)
                {
                    var outStr = "Application global state: ";
                    foreach (var v in app.Params.GlobalState)
                    {
                        outStr += v.ToString();
                    }                    
                    Console.WriteLine(outStr);
                }
            }
        }
    }
}
