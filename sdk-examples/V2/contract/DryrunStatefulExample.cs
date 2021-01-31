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
            // TODO: REMOVE:
            string creatorMnemonic = "benefit once mutual legal marble hurdle dress toe fuel country prepare canvas barrel divide major square name captain calm flock crumble receive economy abandon power";
            string userMnemonic = "pledge become mouse fantasy matrix bunker ask tissue prepare vocal unit patient cliff index train network intact company across stage faculty master mom abstract above";

            var creator = new Account(creatorMnemonic);
            var user = new Account(userMnemonic);

            // declare application state storage (immutable)
            int localInts = 1;
            int localBytes = 1;
            int globalInts = 1;
            int globalBytes = 0;

            // user declared approval program (initial)
            string approvalProgramSourceInitial = "#pragma version 2\n"
                    + "///// Handle each possible OnCompletion type. We don't have to worry about\n"
                    + "//// handling ClearState, because the ClearStateProgram will execute in that\n"
                    + "//// case, not the ApprovalProgram.\n" +

                    "txn OnCompletion\n" + "int NoOp\n" + "==\n" + "bnz handle_noop\n" +

                    "txn OnCompletion\n" + "int OptIn\n" + "==\n" + "bnz handle_optin\n" +

                    "txn OnCompletion\n" + "int CloseOut\n" + "==\n" + "bnz handle_closeout\n" +

                    "txn OnCompletion\n" + "int UpdateApplication\n" + "==\n" + "bnz handle_updateapp\n" +

                    "txn OnCompletion\n" + "int DeleteApplication\n" + "==\n" + "bnz handle_deleteapp\n" +

                    "//// Unexpected OnCompletion value. Should be unreachable.\n" + "err\n" +

                    "handle_noop:\n" + "//// Handle NoOp\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "bnz handle_optin\n" +

                    "//// read global state\n" + "byte \"counter\"\n" + "dup\n" + "app_global_get\n" +

                    "//// increment the value\n" + "int 1\n" + "+\n" +

                    "//// store to scratch space\n" + "dup\n" + "store 0\n" +

                    "//// update global state\n" + "app_global_put\n" +

                    "//// read local state for sender\n" + "int 0\n" + "byte \"counter\"\n" + "app_local_get\n" +

                    "//// increment the value\n" + "int 1\n" + "+\n" + "store 1\n" +

                    "//// update local state for sender\n" + "int 0\n" + "byte \"counter\"\n" + "load 1\n"
                    + "app_local_put\n" +

                    "//// load return value as approval\n" + "load 0\n" + "return\n" +

                    "handle_optin:\n" + "//// Handle OptIn\n" + "//// approval\n" + "int 1\n" + "return\n" +

                    "handle_closeout:\n" + "//// Handle CloseOut\n" + "////approval\n" + "int 1\n" + "return\n" +

                    "handle_deleteapp:\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "return\n" +

                    "handle_updateapp:\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "return\n";

            // user declared approval program (refactored)
            string approvalProgramSourceRefactored = "#pragma version 2\n"
                    + "//// Handle each possible OnCompletion type. We don't have to worry about\n"
                    + "//// handling ClearState, because the ClearStateProgram will execute in that\n"
                    + "//// case, not the ApprovalProgram.\n" +

                    "txn OnCompletion\n" + "int NoOp\n" + "==\n" + "bnz handle_noop\n" +

                    "txn OnCompletion\n" + "int OptIn\n" + "==\n" + "bnz handle_optin\n" +

                    "txn OnCompletion\n" + "int CloseOut\n" + "==\n" + "bnz handle_closeout\n" +

                    "txn OnCompletion\n" + "int UpdateApplication\n" + "==\n" + "bnz handle_updateapp\n" +

                    "txn OnCompletion\n" + "int DeleteApplication\n" + "==\n" + "bnz handle_deleteapp\n" +

                    "//// Unexpected OnCompletion value. Should be unreachable.\n" + "err\n" +

                    "handle_noop:\n" + "//// Handle NoOp\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "bnz handle_optin\n" +

                    "//// read global state\n" + "byte \"counter\"\n" + "dup\n" + "app_global_get\n" +

                    "//// increment the value\n" + "int 1\n" + "+\n" +

                    "//// store to scratch space\n" + "dup\n" + "store 0\n" +

                    "//// update global state\n" + "app_global_put\n" +

                    "//// read local state for sender\n" + "int 0\n" + "byte \"counter\"\n" + "app_local_get\n" +

                    "//// increment the value\n" + "int 1\n" + "+\n" + "store 1\n" +

                    "//// update local state for sender\n" + "//// update \"counter\"\n" + "int 0\n" + "byte \"counter\"\n"
                    + "load 1\n" + "app_local_put\n" +

                    "//// update \"timestamp\"\n" + "int 0\n" + "byte \"timestamp\"\n" + "txn ApplicationArgs 0\n"
                    + "app_local_put\n" +

                    "//// load return value as approval\n" + "load 0\n" + "return\n" +

                    "handle_optin:\n" + "//// Handle OptIn\n" + "//// approval\n" + "int 1\n" + "return\n" +

                    "handle_closeout:\n" + "//// Handle CloseOut\n" + "////approval\n" + "int 1\n" + "return\n" +

                    "handle_deleteapp:\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "return\n" +

                    "handle_updateapp:\n" + "//// Check for creator\n"
                    + "addr 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4\n" + "txn Sender\n" + "==\n"
                    + "return\n";
            // creator 53GNUYJSTKGEHAVYE5ZS65YTVJSYZSJ7KJBWNQT3MJESCOKNOWEBYTLVA4
            // user GG7UDCTXNHADKSJ22GG64BZNKXXLXMSYWVZDD2UGHBZ6RLVXWGRLMW52DU
            // declare clear state program source
            string clearProgramSource = "#pragma version 2\n" + "int 1\n";

            var algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            var approval_program_compiled =
                algodApiInstance.TealCompile(approvalProgramSourceInitial);
            var clear_program_compiled =
                algodApiInstance.TealCompile(approvalProgramSourceRefactored);
            var approval_program_refactored_compiled =
                algodApiInstance.TealCompile(clearProgramSource);

            try
            {
                var appid = CreateApp(algodApiInstance, creator, new TEALProgram(approval_program_compiled.Result),
                new TEALProgram(clear_program_compiled.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                OptIn(algodApiInstance, user, appid);



                // call application without arguments
                CallApp(algodApiInstance, user, appid, null);

                // read local state of application from user account
                ReadLocalState(algodApiInstance, user, appid);

                // read global state of application
                ReadGlobalState(algodApiInstance, creator, appid);

                // update application
                approvalProgram = compileProgram(client, approvalProgramSourceRefactored.getBytes("UTF-8"));
                updateApp(client, creatorAccount, appId, new TEALProgram(approvalProgram), new TEALProgram(clearProgram));

                // call application with arguments
                SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd 'at' HH:mm:ss");
                Date date = new Date(System.currentTimeMillis());
                System.out.println(formatter.format(date));
                List<byte[]> appArgs = new ArrayList<byte[]>();
                appArgs.add(formatter.format(date).toString().getBytes("UTF8"));
                callApp(client, userAccount, appId, appArgs);

                // read local state of application from user account
                readLocalState(client, userAccount, appId);

                // close-out from application
                closeOutApp(client, userAccount, appId);

                // opt-in again to application
                optInApp(client, userAccount, appId);

                // call application with arguments
                callApp(client, userAccount, appId, appArgs);

                // read local state of application from user account
                readLocalState(client, userAccount, appId);

                // delete application
                deleteApp(client, creatorAccount, appId);

                // clear application from user account
                clearApp(client, userAccount, appId);

















































                Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
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
                    Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
                    Console.WriteLine("Application ID is: " + JObject.Parse(resp.Txn.ToString())["apid"]);
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
                    Console.WriteLine("Optin to Application ID: " + resp.ApplicationIndex);
                }
                catch (ApiException e)
                {
                    Console.WriteLine("Exception when calling create application: " + e.Message);
                }
            }

            static void deleteApp(AlgodApi client, Account sender, long? applicationId)
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
                    Console.WriteLine("Optin to Application ID: " + resp.ApplicationIndex);
                }
                catch (ApiException e)
                {
                    Console.WriteLine("Exception when calling create application: " + e.Message);
                }
            }

            static void clearApp(AlgodApi client, Account sender, long? applicationId)
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
                    Console.WriteLine("Optin to Application ID: " + resp.ApplicationIndex);
                }
                catch (ApiException e)
                {
                    Console.WriteLine("Exception when calling create application: " + e.Message);
                }
            }

            static void CallApp(AlgodApi client, Account sender, long? applicationId, List<byte[]> args, TEALProgram program, string tealFileName)
            {
                try
                {
                    var transParams = client.TransactionParams();
                    var tx = Utils.GetApplicationCallTransaction(sender.Address, (ulong?)applicationId, transParams, args);
                    var signedTx = sender.SignTransaction(tx);
                    Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                    var cr = client.AccountInformation(creator.Address.ToString());
                    var usr = client.AccountInformation(user.Address.ToString());
                    var mydrr = Dryrun_drr(signedTx, program, cr, usr);
                    Write_drr("mydrr.dr", mydrr);
                    Console.WriteLine("debugger starting...");
                    //# START debugging session
                    //# either use from terminal in this folder
                    //# `tealdbg debug program.teal --dryrun-req mydrr.dr`
                    //#
                    //# or use this line to invoke debugger
                    //# and switch to chrome://inspect to inspect and debug
                    //# (program execution will continue aafter debuigging session completes)






                    var id = Utils.SubmitTransaction(client, signedTx);
                    Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                    var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                    Console.WriteLine("Confirmed at round: " + resp.ConfirmedRound);
                    Console.WriteLine("Application ID is: " + JObject.Parse(resp.Txn.ToString())["apid"]);
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

            static DryrunRequest Dryrun_drr(SignedTransaction signTx, TEALProgram program, Algorand.V2.Model.Account cr, Algorand.V2.Model.Account usr)
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
                        Console.WriteLine("User's application local state: " + applicationLocalState[i].KeyValue.ToString());
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
                        Console.WriteLine("Application global state: " + createdApplications[i].Params.GlobalState.ToString());
                    }
                }
            }
        }
    }
