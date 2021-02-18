using Algorand;
using Algorand.Client;
using Algorand.V2;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Algorand.V2.Model;
using Account = Algorand.Account;

namespace sdk_examples.V2.contract
{
    class DryrunStatefulExample
    {
        public static void Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
            var client = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            //// TODO: REMOVE:
            string creatorMnemonic = "benefit once mutual legal marble hurdle dress toe fuel country prepare canvas barrel divide major square name captain calm flock crumble receive economy abandon power";
            //string userMnemonic = "pledge become mouse fantasy matrix bunker ask tissue prepare vocal unit patient cliff index train network intact company across stage faculty master mom abstract above";

            ulong localInts = 1;
            ulong localBytes = 0;
            ulong globalInts = 1;
            ulong globalBytes = 0;

            byte[] data = File.ReadAllBytes("V2/contract/hello_world.teal");
            var approval_program_compiled = client.TealCompile(data);
            data = File.ReadAllBytes("V2/contract/hello_world_clear.teal");
            var clear_program_compiled = client.TealCompile(data);
            data = File.ReadAllBytes("V2/contract/hello_world_updated.teal");
            var approval_program_refactored_compiled = client.TealCompile(data);

            var admin = new Account(creatorMnemonic);
            var creator = new Account();
            var user = new Account();
            try
            {
                // transfer to creator and user
                var transParams = client.TransactionParams();
                var amount = Utils.AlgosToMicroalgos(1);
                var tx = Utils.GetPaymentTransaction(admin.Address, creator.Address, amount, "", transParams);
                var signedTx = admin.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to creator at round: " + resp.ConfirmedRound);

                tx = Utils.GetPaymentTransaction(admin.Address, user.Address, amount, "", transParams);
                signedTx = admin.SignTransaction(tx);
                id = Utils.SubmitTransaction(client, signedTx);
                resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to user at round: " + resp.ConfirmedRound);

                var appid = CreateApp(client, creator, new TEALProgram(approval_program_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                OptIn(client, user, appid);
                OptIn(client, creator, appid);

                //call app from user account updates global storage

                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_compiled.Result), "V2/contract/hello_world.teal");

                // read global state of application
                ReadGlobalState(client, creator, appid);

                // update application
                UpdateApp(client, creator, appid,
                    new TEALProgram(approval_program_refactored_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result));

                //call application with updated app which updates local storage counter
                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "V2/contract/hello_world_updated.teal");

                //read local state of application from user account
                ReadLocalState(client, user, appid);

                //close-out from application - removes application from balance record
                CloseOutApp(client, user, (ulong?)appid);

                //opt-in again to application
                OptIn(client, user, appid);

                //call application with arguments
                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "V2/contract/hello_world_updated.teal");

                // delete application
                // clears global storage only
                // user must clear local
                DeleteApp(client, creator, appid);

                // clear application from user account
                // clears local storage
                ClearApp(client, user, appid);
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }

        public static void CloseOutApp(AlgodApi client, Account sender, ulong? appId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCloseTransaction(sender.Address, appId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Close out Application ID is: " + appId);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static void UpdateApp(AlgodApi client, Account creator, long? appid, TEALProgram approvalProgram, TEALProgram clearProgram)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationUpdateTransaction(creator.Address, (ulong?)appid, approvalProgram, clearProgram, transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Updated the application ID is: " + appid);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static long? CreateApp(AlgodApi client, Account creator, TEALProgram approvalProgram,
            TEALProgram clearProgram, ulong? globalInts, ulong? globalBytes, ulong? localInts, ulong? localBytes)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCreateTransaction(creator.Address, approvalProgram, clearProgram,
                new StateSchema(globalInts, globalBytes), new StateSchema(localInts, localBytes), transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Application ID is: " + resp.ApplicationIndex);
                return resp.ApplicationIndex;
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
                return null;
            }
        }

        static void OptIn(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationOptinTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Optin to Application ID: " + (resp.Txn as JObject)["txn"]["apid"]);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void DeleteApp(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationDeleteTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Success deleted the application " + (resp.Txn as JObject)["txn"]["apid"]);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void ClearApp(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationClearTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Success cleared the application " + (resp.Txn as JObject)["txn"]["apid"]);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void CallApp(AlgodApi client, Account creator, Account user, long? applicationId, List<byte[]> args,
            TEALProgram program, string tealFileName)
        {
            Console.WriteLine("Creator Account:" + creator.Address.ToString());
            Console.WriteLine("User Account:" + user.Address.ToString());
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCallTransaction(user.Address, (ulong?)applicationId, transParams, args);
                var signedTx = user.SignTransaction(tx);
                //Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var cr = client.AccountInformation(creator.Address.ToString());
                var usr = client.AccountInformation(user.Address.ToString());
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

                Excute(string.Format("tealdbg debug {0} --dryrun-req {1}", tealFileName, drrFile));

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
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
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static void Excute(string line)
        {
            var strCmdText = "/C " + line;
            System.Diagnostics.Process.Start("cmd.exe", strCmdText);
        }

        private static void WriteDrr(string filePath, DryrunRequest content)
        {
            var data = Encoder.EncodeToMsgPack(content);
            File.WriteAllBytes(filePath, data);
        }

        static DryrunRequest DryrunDrr(SignedTransaction signTx, TEALProgram program, Algorand.V2.Model.Account cr, Algorand.V2.Model.Account usr)
        {
            var sources = new List<DryrunSource>();

            if (program != null)
            {
                sources.Add(new DryrunSource("approv", Convert.ToBase64String(program.Bytes), 0));
            }
            var drr = new DryrunRequest(new List<SignedTransaction>() { signTx },
                new List<Algorand.V2.Model.Account>() { cr, usr }, sources: sources);
            return drr;
        }

        static public void ReadLocalState(AlgodApi client, Account account, long? appId)
        {
            var acctResponse = client.AccountInformation(account.Address.ToString());
            var applicationLocalState = acctResponse.AppsLocalState;
            for (int i = 0; i < applicationLocalState.Count; i++)
            {
                if (applicationLocalState[i].Id == appId)
                {
                    var outStr = "User's application local state: ";
                    foreach (var v in applicationLocalState[i].KeyValue)
                    {
                        outStr += v.ToString();
                    }
                    Console.WriteLine(outStr);
                }
            }
        }

        static public void ReadGlobalState(AlgodApi client, Account account, long? appId)
        {
            var acctResponse = client.AccountInformation(account.Address.ToString());
            var createdApplications = acctResponse.CreatedApps;
            for (int i = 0; i < createdApplications.Count; i++)
            {
                if (createdApplications[i].Id == appId)
                {
                    var outStr = "Application global state: ";
                    foreach (var v in createdApplications[i].Params.GlobalState)
                    {
                        outStr += v.ToString();
                    }
                    Console.WriteLine(outStr);
                }
            }
        }
        
    }
}
