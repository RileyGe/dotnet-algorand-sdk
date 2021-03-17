using Algorand;
using Algorand.Client;
using Algorand.V2;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Algorand.V2.Model;
using Account = Algorand.Account;
using System.Text;

namespace sdk_examples.V2.contract
{
    class StatefulContract
    {
        public static void Main(params string[] args)
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

            var client = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

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

            var approvalProgram =
                client.TealCompile(Encoding.UTF8.GetBytes(approvalProgramSourceInitial));
            var clearProgram =
                client.TealCompile(Encoding.UTF8.GetBytes(clearProgramSource));
            var approvalProgramRefactored =
                client.TealCompile(Encoding.UTF8.GetBytes(approvalProgramSourceRefactored));

            try
            {
                // create new application
                var appid = CreateApp(client, creator, new TEALProgram(approvalProgram.Result),
                    new TEALProgram(clearProgram.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                OptIn(client, user, appid);
                // call application without arguments
                CallApp(client, user, appid, null);
                // read local state of application from user account
                ReadLocalState(client, user, appid);

                // read global state of application
                ReadGlobalState(client, creator, appid);

                // update application
                UpdateApp(client, creator, appid,
                    new TEALProgram(approvalProgramRefactored.Result),
                    new TEALProgram(clearProgram.Result));
                // call application with arguments
                var date = DateTime.Now;
                Console.WriteLine(date.ToString("yyyy-MM-dd 'at' HH:mm:ss"));
                List<byte[]> appArgs = new List<byte[]>
                {
                    Encoding.UTF8.GetBytes(date.ToString("yyyy-MM-dd 'at' HH:mm:ss"))
                };
                CallApp(client, user, appid, appArgs);

                // read local state of application from user account
                ReadLocalState(client, user, appid);

                // close-out from application
                CloseOutApp(client, user, (ulong)appid);

                // opt-in again to application
                OptIn(client, user, appid);

                // call application with arguments
                CallApp(client, user, appid, appArgs);

                // read local state of application from user account
                ReadLocalState(client, user, appid);

                // delete application
                DeleteApp(client, creator, appid);

                // clear application from user account
                ClearApp(client, user, appid);

                Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#sendTransaction: " + e.Message);
            }
        }
        public static void CloseOutApp(AlgodApi client, Account sender, ulong appId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCloseTransaction(sender.Address, (ulong?)appId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
                Console.WriteLine("Application ID is: " + appId);
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
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
                Console.WriteLine("Application ID is: " + appid);
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
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Application ID is: " + resp.ApplicationIndex.ToString());
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
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine(string.Format("Address {0} optin to Application({1})",
                    sender.Address.ToString(), (resp.Txn as JObject)["txn"]["apid"]));
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

        static void CallApp(AlgodApi client, Account sender, long? applicationId, List<byte[]> args)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCallTransaction(sender.Address, (ulong?)applicationId, transParams, args);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
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
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
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
