using Algorand;
using Algorand.Client;
using Algorand.V2;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Algorand.V2.Algod.Model;
using Account = Algorand.Account;
using System.Runtime.InteropServices;
using Algorand.V2.Algod;
using System.Threading.Tasks;

namespace sdk_examples.V2.contract
{
    class DryrunStatefulExample
    {
        public async Task Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            var client = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            //// TODO: REMOVE:
            string creatorMnemonic = "benefit once mutual legal marble hurdle dress toe fuel country prepare canvas barrel divide major square name captain calm flock crumble receive economy abandon power";
            //string userMnemonic = "pledge become mouse fantasy matrix bunker ask tissue prepare vocal unit patient cliff index train network intact company across stage faculty master mom abstract above";

            ulong localInts = 1;
            ulong localBytes = 0;
            ulong globalInts = 1;
            ulong globalBytes = 0;

            byte[] data = File.ReadAllBytes("V2/contract/hello_world.teal");
            CompileResponse approval_program_compiled;
            using (var datams = new MemoryStream(data))
            {
                approval_program_compiled = await client.CompileAsync(datams);
            }
            data = File.ReadAllBytes("V2/contract/hello_world_clear.teal");
            CompileResponse clear_program_compiled;
            using (var datams = new MemoryStream(data))
            {
                clear_program_compiled = await client.CompileAsync(datams);
            }
            data = File.ReadAllBytes("V2/contract/hello_world_updated.teal");
            CompileResponse approval_program_refactored_compiled;
            using (var datams = new MemoryStream(data))
            {
                approval_program_refactored_compiled = await client.CompileAsync(datams);
            }

            var admin = new Account(creatorMnemonic);
            var creator = new Account();
            var user = new Account();
            try
            {
                // transfer to creator and user
                var transParams = await client.ParamsAsync();
                var amount = Utils.AlgosToMicroalgos(1);
                var tx = Utils.GetPaymentTransaction(admin.Address, creator.Address, amount, "", transParams);
                var signedTx = admin.SignTransaction(tx);
                var id = await Utils.SubmitTransaction(client, signedTx);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to creator at round: " + resp.ConfirmedRound);

                tx = Utils.GetPaymentTransaction(admin.Address, user.Address, amount, "", transParams);
                signedTx = admin.SignTransaction(tx);
                id = await Utils.SubmitTransaction(client, signedTx);
                resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to user at round: " + resp.ConfirmedRound);

                var appid = await CreateApp(client, creator, new TEALProgram(approval_program_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                await OptIn(client, user, appid);
                await OptIn(client, creator, appid);

                //call app from user account updates global storage

                await CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_compiled.Result), "V2/contract/hello_world.teal");

                // read global state of application
                await ReadGlobalState(client, creator, appid);

                // update application
                await UpdateApp(client, creator, appid,
                    new TEALProgram(approval_program_refactored_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result));

                //call application with updated app which updates local storage counter
                await CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "V2/contract/hello_world_updated.teal");

                //read local state of application from user account
                await ReadLocalState(client, user, appid);

                //close-out from application - removes application from balance record
                await CloseOutApp(client, user, (ulong?)appid);

                //opt-in again to application
                await OptIn(client, user, appid);

                //call application with arguments
                await  CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "V2/contract/hello_world_updated.teal");

                // delete application
                // clears global storage only
                // user must clear local
                await DeleteApp(client, creator, appid);

                // clear application from user account
                // clears local storage
                await ClearApp(client, user, appid);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }

        public async static Task CloseOutApp(DefaultApi client, Account sender, ulong? appId)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCloseTransaction(sender.Address, appId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Close out Application ID is: " + appId);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private async static Task UpdateApp(DefaultApi client, Account creator, ulong? appid, TEALProgram approvalProgram, TEALProgram clearProgram)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationUpdateTransaction(creator.Address, (ulong?)appid, approvalProgram, clearProgram, transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Updated the application ID is: " + appid);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        async static Task<ulong?> CreateApp(DefaultApi client, Account creator, TEALProgram approvalProgram,
            TEALProgram clearProgram, ulong globalInts, ulong globalBytes, ulong localInts, ulong localBytes)
        {
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCreateTransaction(creator.Address, approvalProgram, clearProgram,
                new Algorand.V2.Indexer.Model.StateSchema() { NumUint = globalInts, NumByteSlice = globalBytes }, new Algorand.V2.Indexer.Model.StateSchema() { NumUint = localInts, NumByteSlice = localBytes }, transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Application ID is: " + resp.ApplicationIndex);
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
                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Optin to Application ID: " + (resp.Txn as JObject)["txn"]["apid"]);
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
                var tx = Utils.GetApplicationDeleteTransaction(sender.Address, applicationId, transParams);
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
                var tx = Utils.GetApplicationClearTransaction(sender.Address,applicationId, transParams);
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

        async static Task  CallApp(DefaultApi client, Account creator, Account user, ulong? applicationId, List<byte[]> args,
            TEALProgram program, string tealFileName)
        {
            Console.WriteLine("Creator Account:" + creator.Address.ToString());
            Console.WriteLine("User Account:" + user.Address.ToString());
            try
            {
                var transParams = await client.ParamsAsync();
                var tx = Utils.GetApplicationCallTransaction(user.Address, (ulong?)applicationId, transParams, args);
                var signedTx = user.SignTransaction(tx);
                //Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var cr = await client.AccountsAsync(creator.Address.ToString(),null);
                var usr = await client.AccountsAsync(user.Address.ToString(),null);
                var mydrr = DryrunDrr(signedTx, program, cr, usr);
                var drrFile = "mydrr.dr";
                WriteDrr(drrFile, mydrr);
                Console.WriteLine("drr file created ... debugger starting - goto chrome://inspect");

                // START debugging session
                // either use from terminal in this folder
                // `tealdbg debug program.teal --dryrun-req mydrr.dr`
                //
                // or use this line to invoke debugger
                // and switch to chrome://inspect to inspect and debug
                // (program execution will continue aafter debuigging session completes)

                Execute(string.Format("tealdbg debug {0} --dryrun-req {1}", tealFileName, drrFile));

                // break here on the next line with debugger
                // run this command in this folder
                // tealdbg debug hello_world.teal --dryrun-req mydrr.dr
                // or
                // tealdbg debug hello_world_updated.teal --dryrun-req mydrr.dr

                var id = await Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed at round: " + resp.ConfirmedRound);
                //System.out.println("Called app-id: " + pTrx.txn.tx.applicationId);
                if (resp.GlobalStateDelta != null)
                {
                    Console.WriteLine("    Global state: " + resp.GlobalStateDelta.ToString());
                }
                if (resp.LocalStateDelta != null)
                {
                    Console.WriteLine("    Local state: " + resp.LocalStateDelta.ToString());
                }
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static void Execute(string line)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Diagnostics.Process.Start("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal", line);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var strCmdText = "/C " + line;
                System.Diagnostics.Process.Start("cmd.exe", strCmdText);
            }            
        }

        private static void WriteDrr(string filePath, DryrunRequest content)
        {
            var data = Encoder.EncodeToMsgPack(content);
            File.WriteAllBytes("./V2/contract/" + filePath, data);
        }

        static DryrunRequest DryrunDrr(SignedTransaction signTx, TEALProgram program, Algorand.V2.Algod.Model.Account cr, Algorand.V2.Algod.Model.Account usr)
        {
            var sources = new List<DryrunSource>();

            if (program != null)
            {
                sources.Add(new DryrunSource() { FieldName = "approv",Source= Convert.ToBase64String(program.Bytes),TxnIndex=0 } );
            }
            var drr = new DryrunRequest()
            {
                Txns = new List<SignedTransaction>() { signTx },
                Accounts = new List<Algorand.V2.Algod.Model.Account>() { cr, usr },
                Sources = sources
            };
            return drr;
        }

        async static public Task ReadLocalState(DefaultApi client, Account account, ulong? appId)
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

        async static public Task ReadGlobalState(DefaultApi client, Account account, ulong? appId)
        {
            var acctResponse = await client.AccountsAsync(account.Address.ToString(),null);
            var createdApplications = acctResponse.CreatedApps;
            foreach (var app in createdApplications)
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
